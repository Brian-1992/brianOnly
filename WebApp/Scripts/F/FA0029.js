Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    var T1GetExcel = '../../../api/FA0029/Excel';
    var reportUrl = '/Report/F/FA0029.aspx';
    

    function SetDate() {
        nowDate = new Date();
        nowDate.getMonth();
        nowDate = Ext.Date.format(nowDate, "Ymd") - 19110000;
        nowDate = nowDate.toString().substring(0, 5);
        T1Query.getForm().findField('P1').setValue(nowDate);
    }


    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [

                {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P1',
                    id: 'P1',
                    width: 170,
                    labelWidth: 70,
                    fieldCls: 'required',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
               
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        if (f.isValid()) {
                            T1Load();
                        }
                        else {

                            Ext.MessageBox.alert('提示', '請輸入必填欄位');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                      
                        T1Query.getForm().findField('P1').reset();
                      
                        var f = this.up('form').getForm();
                        f.findField('P1').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                },

            ],
        }],

    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['APPDEPT', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'APPDEPT_NAME,', 'SUM_APV_VOLUME']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'APPDEPT', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                  
                    p1: T1Query.getForm().findField('P1').rawValue,
               
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0029/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });


    function T1Load() {
        T1Tool.moveFirst();
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            beforechange: function (T1Tool, pageData) {
                T1Rec = 0; //disable編修按鈕&刪除按鈕
                T1LastRec = null; //T1Form之資料輸選區清空
            },
            afterrender: function (T1Tool) {
                T1Tool.emptyMsg = '<font color=red>沒有任何資料</font>';
            }
        },
        buttons: [
            {
                itemId: 't1print', text: '列印', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('列印', '是否確定列印？', function (btn, text) {
                        if (btn === 'yes') {
                            showReport();
                        }
                    });
                }
            }, {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                          
                            p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                         
                            PostForm(T1GetExcel, p);
                        }
                    });
                }

            },
        ]
    });

   
   
    function showReport() {
        if (!win) {
            var np = {

                p1: T1Query.getForm().findField('P1').rawValue,

            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                //html: '<iframe src="' + reportUrl + '?WH_NO=' + WH_NO + '&STORE_LOC=' + STORE_LOC + '&BARCODE_IsUsing=' + BARCODE_IsUsing + '&STATUS=' + STATUS + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',

                html: '<iframe src="' + reportUrl + '?p1=' + np.p1 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
               // html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',

                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "成本碼",
                dataIndex: 'APPDEPT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "單位名稱 ",
                dataIndex: 'APPDEPT_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 120
            }, {
                text: "英文品名 ",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 250
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 250

            }, {
                text: "申領品項總材積",
                dataIndex: 'SUM_APV_VOLUME',
                style: 'text-align:left',
                align: 'right',
                width: 100
           

            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });

    T1Query.getForm().findField('D0').focus();
    SetDate();
});