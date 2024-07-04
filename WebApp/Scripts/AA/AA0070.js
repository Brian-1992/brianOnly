Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

var MATComboGet = '../../../api/AA0070/GetMATCombo';
var matUserID;

// 物品類別清單
var MATQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

var T1GetExcel = '/api/AA0070/Excel';
var reportUrl = '/Report/A/AA0070.aspx';

var P0 = '';
var P1 = '';
var P2 = '';
var P3 = '';


Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });



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
                    xtype: 'displayfield',
                    fieldLabel: '物料分類',
                    value:'02 衛材'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '調撥日期',
                    name: 'P1',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 4 0 4',
                    editable: false
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    labelWidth: 8,
                    width: 88,
                    padding: '0 4 0 4',
                    labelSeparator: '',
                    editable: false
                }, {
                    xtype: 'datefield',
                    fieldLabel: '申請日期',
                    name: 'P4',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 4 0 4',
                    editable: false
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P5',
                    labelWidth: 8,
                    width: 88,
                    padding: '0 4 0 4',
                    labelSeparator: '',
                    editable: false
                },{
                    xtype: 'fieldcontainer',
                    fieldLabel: '僅顯示未完成歸墊',
                    defaultType: 'checkboxfield',
                    labelWidth: 110,
                    labelSeparator: '',
                    width: 150,
                    items: [
                        {
                            name: 'P3',
                            inputValue: '1',
                        }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = T1Query.getForm();
                        P0 = '02';
                        P1 = f.findField('P1').rawValue;
                        P2 = f.findField('P2').rawValue;
                        P3 = f.findField('P3').checked;
                        P4 = f.findField('P4').rawValue;
                        P5 = f.findField('P5').rawValue;
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('WH_NO').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }


            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0070', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    P4: P4,
                    P5: P5,
                };                
                Ext.apply(store.proxy.extraParams, np);

            },
        }
    });



    function T1Load() {

        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1}); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'P4', value: P4 }); //SQL篩選條件                            
                            p.push({ name: 'P5', value: P5 }); //SQL篩選條件                            
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
            {
                id: 't1print', text: '列印', disabled: false, handler: function () {
                    showReport();
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
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
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 160
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 80
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 100
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            }, {
                text: "調撥量",
                dataIndex: 'BW_MQTY',
                style: 'text-align:left',
                align: 'right',
                width: 60
            }, {
                text: "歸墊量",
                dataIndex: 'RV_MQTY',
                style: 'text-align:left',
                align: 'right',
                width: 60
            }, {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 60
            }, {
                text: "調撥日期",
                dataIndex: 'DIS_TIME',
                width: 60
            }, {
                header: "",
                flex: 1
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
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false, 
                html: '<iframe src="' + reportUrl + '?MAT_CLASS=' + P0 + '&DIS_TIME_B=' + P1 + '&DIS_TIME_E=' + P2 + '&P3=' + P3 + '&APPTIME_B=' + P4 + '&APPTIME_E=' + P5 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        }
        ]
    });


});
