Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    //var T1Name = "藥局進貨作業";
    var T1Rec = 0;
    var T1LastRec = null;
    //var IsPageLoad = true;
    // var pSize = 100; //分頁時每頁筆數
    var St_MatclassGet = '../../../api/AB0058/GetMatclassCombo';
    var T1GetExcel = '../../../api/AB0058/GetExcel';
    var reportUrl = '/Report/A/AB0058.aspx';
    var col1_labelWid = 130;
    var col1_Wid = 280;
    var col2_labelWid = 130;
    var col2_Wid = 260;
    var col3_labelWid = 110;
    var col3_Wid = 300;
    var f2_wid = (col1_Wid + col2_Wid + col3_Wid) / 6;
    var f3_wid = (col1_Wid + col2_Wid + col3_Wid) / 4;
    var f4_wid = (col1_Wid + col2_Wid + col3_Wid) / 5;
    var mLabelWidth = 70;
    var mWidth = 180;
    var Dno;

    var mmCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P2',
        id: 'P2',
        name: 'MMCODE',
        fieldLabel: '院內碼',

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0041/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數

        //listeners: {
        //    select: function (c, r, i, e) {
        //        //選取下拉項目時，顯示回傳值
        //        alert(r.get('MAT_CLASS'));
        //    }
        //}
    });

    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var FlowidQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM'],
        data: [
            { "KEY_CODE": "RN1", "COMBITEM": "RN1 - 繳回中央庫房" },
            { "KEY_CODE": "TR1", "COMBITEM": "TR1 - 調撥至其他衛星庫" },
            { "KEY_CODE": "AJ1", "COMBITEM": "AJ1 - 其他調帳" },
              ]
    });



    function getDefaultValue(isEndDate) {
        var yyyy = 0;
        var m = 0;
        if (isEndDate == "D1") {
            yyyy = new Date().getFullYear() - 1911;
            m = new Date().getMonth() + 1;
        } else if (isEndDate == "D0") {    //減6個月
            var date = new Date();
            date.setMonth(date.getMonth() - 11);

            yyyy = date.getFullYear() - 1911;
            m = date.getMonth();
            if (m == 0) {   //因為從目前六月算起，的前六月是12月，但它跑出來是0
                yyyy = yyyy - 1;
                m = 12;
            }
        }

        var d = 0;
        d = new Date().getDate();

        var mm = m > 10 ? m.toString() : "0" + m.toString();
        var dd = d > 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;

    }

    function setComboData() {
        Ext.Ajax.request({
            url: St_MatclassGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_matclass = data.etts;


                    if (tb_matclass.length > 0) {
                        for (var i = 0; i < tb_matclass.length; i++) {
                            st_matclass.add({ VALUE: tb_matclass[i].VALUE, COMBITEM: tb_matclass[i].COMBITEM });
                        }
                    }

                }
            },
            failure: function (response, options) {

            }
        });

    }
    setComboData();


    var mLabelWidth = 70;
    var mWidth = 230;
    var T1QueryForm = Ext.widget({
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
                    xtype: 'datefield',
                    fieldLabel: '出貨日期區間',
                    name: 'D0',
                    id: 'D0',
                    vtype: 'dateRange',
                    dateRange: { end: 'D1' },
                    value: getDefaultValue("D0"),
                    labelWidth: 120,
                    width: 220
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'D1',
                    id: 'D1',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'D0' },
                    value: getDefaultValue("D1"),
                    labelWidth: 7,
                    width: 107
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P0',
                    id: 'P0',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true,
                    queryMode: 'local',
                    labelWidth: 60,
                    width: 180,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                   
                }, {
                    xtype: 'combo',
                    store: FlowidQueryStore,
                    displayField: 'COMBITEM',
                    valueField: 'KEY_CODE',
                    queryMode: 'local',
                    matchFieldWidth: false,

                    autoSelect: true,
                    //multiSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    fieldLabel: '單據類別',
                    name: 'P1',
                    id: 'P1',
                    width: 200,
                    padding: '0 4 0 4',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 90
                }, mmCode            
                , {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1QueryForm.getForm().isValid()) {
                            T1Load();
                        }


                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('D0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
      
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'DOCNO', 'DOCTYPE', 'TOWH', 'MAT_CLASS', 'APPID', 'APPTIME', 'M_CONTPRICE', 'BASE_UNIT', 'APPQTY', 'MMNAME_C']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'APPTIME', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T1QueryForm.getForm().findField('D0').rawValue,
                    p2: T1QueryForm.getForm().findField('D1').rawValue,
                    p3: T1QueryForm.getForm().findField('P0').getValue(),
                    p4: T1QueryForm.getForm().findField('P1').getValue(),
                    p5: T1QueryForm.getForm().findField('P2').getValue(),
                                    };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0058/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });



    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '匯出', border: 1,
                style: {
                    borderColor: '#0080ff',
                    borderStyle: 'solid'
                },
                handler: function () {
                    var p = new Array();

                    p.push({ name: 'd0', value: T1QueryForm.getForm().findField('D0').rawValue }); //使用getValue格式會有不同格式
                    p.push({ name: 'd1', value: T1QueryForm.getForm().findField('D1').rawValue });
                    p.push({ name: 'p0', value: T1QueryForm.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1QueryForm.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1QueryForm.getForm().findField('P2').getValue() });

                    PostForm('/api/AB0058/GetExcel', p);
                    msglabel('訊息區:匯出完成');

                }
            }, {
                text: '列印', handler: function () {
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可列');
                    }
                }
          
            }
        ]
    });



    //Ext.tip.QuickTipManager.init();

    var T1Grid = Ext.create('Ext.grid.Panel', {
        menuDisabled: true,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1QueryForm]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
       
        columns: [
            { xtype: 'rownumberer' },
            { text: '單號', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 160 },
            { text: '申請日期', dataIndex: 'APPTIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '出貨種類', dataIndex: 'DOCTYPE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '入庫房', dataIndex: 'TOWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '物料分類', dataIndex: 'MAT_CLASS', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '品名', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 260 },
            { text: '出貨數量', dataIndex: 'APPQTY', align: 'right', style: 'text-align:left', menuDisabled: true, width: 80 },
            { text: '單位', dataIndex: 'BASE_UNIT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 50 },
            { text: '平均單價', dataIndex: 'M_CONTPRICE', align: 'right', style: 'text-align:left', menuDisabled: true, width: 80 },

        ],

    });
    function showReport() {
        if (!win) {
            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
            

            var np = {
                p1: T1QueryForm.getForm().findField('D0').rawValue,
                p2: T1QueryForm.getForm().findField('D1').rawValue,
                p3: T1QueryForm.getForm().findField('P0').getValue(),
                p4: T1QueryForm.getForm().findField('P1').getValue(),
                p5: T1QueryForm.getForm().findField('P2').getValue(),
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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


    function T1Load() {
        T1Tool.moveFirst();
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    function T2Load() {
        T2Store.load({
            params: {
                p0: Dno,
            }
        });
    }


    var viewport = Ext.create('Ext.Viewport', {
        id: 'viewport',
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
        }]
    });

    //T1Load();
});