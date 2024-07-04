Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Name = "藥品自辦購案管制報表";
    var T1GetExcel = '../../../api/FA0061/Excel';
    var T1GetExcel_Detail = '../../../api/FA0061/Excel_Detail';
    var T21GetExcel = '../../../api/FA0061/MmcodeExcel';

    var mLabelWidth = 70;
    var mWidth = 150;
    var windowHeight = $(window).height();

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },                  //院內碼
            { name: 'MMNAME_E', type: 'string' },                //藥品英文名稱   
            { name: 'SELF_CONTRACT_NO', type: 'string' },        //合約案號
            { name: 'SELF_PUR_UPPER_LIMIT', type: 'string' },    //採購上限金額
            { name: 'SELF_CONT_BDATE', type: 'string' },         //合約生效起日

            { name: 'SELF_CONT_EDATE', type: 'string' },         //合約生效迄日
            { name: 'sum_PAY_AMT', type: 'string' },             //累計結報金額
            { name: 'inqym', type: 'string' }                    //查詢年月
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0061/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,   //查詢年月                      INQYM
                    p1: T1Query.getForm().findField('P1').getValue(), //合約期間累計結報金額 >= ?? 萬    sum_PAY_AMT
                    p2: T1Query.getForm().findField('P2').getValue(), //合約期間累計結報金額>=採購上限金額 is_SELF_PUR_UPPER_LIMIT
                    p3: T1Query.getForm().findField('P3').getValue(), //核取方塊 合約到期前 ?? 月        is_SELF_CONT_BDATE
                    p4: T1Query.getForm().findField('P4').getValue()  //合約到期前 ?? 月               SELF_CONT_BDATE
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('T1xls').setDisabled(false);
                    Ext.getCmp('T1Detailxls').setDisabled(false);
                } else {
                    Ext.getCmp('T1xls').setDisabled(true);
                    Ext.getCmp('T1Detailxls').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // 查詢欄位
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
                    name: 'PT',
                    id: 'PT',
                    xtype: 'hidden'
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '查詢年月',
                    name: 'P0',
                    id: 'P0',
                    fieldCls: 'required',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4',
                    value: new Date()
                }, {
                    xtype: 'numberfield',
                    fieldLabel: '合約期間累計結報金額 >=',
                    name: 'P1',
                    labelWidth: 140,
                    width: 180,
                    labelSeparator: '',
                    submitValue: true,
                    allowBlank: false,
                    fieldCls: 'required', //必填
                    hideTrigger: true,
                    minValue: 1,
                    maxValue: 100,
                    value: 0,//預設值
                    labelAlign: 'right',
                }, {
                    xtype: 'displayfield',
                    labelAlign: 'left',
                    labelSeparator: '',
                    value: '萬',
                    width: 10
                }, {
                    xtype: 'container',
                    defaultType: 'checkboxfield',
                    layout: 'hbox',
                    items: [
                        {
                            boxLabel: '合約期間累計結報金額>=採購上限金額',
                            name: 'P2',
                            inputValue: 'Y',
                            id: 'P2',
                            width: 250,
                            padding: '0 0 0 20',
                        }
                    ]
                }, {
                    xtype: 'container',
                    defaultType: 'checkboxfield',
                    layout: 'hbox',
                    items: [
                        {
                            boxLabel: '合約到期前',
                            name: 'P3',
                            inputValue: 'Y',
                            id: 'P3',
                            width: 80
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '',
                            name: 'P4',
                            labelWidth: 70,
                            width: 30,
                            labelSeparator: '',
                            submitValue: true,
                            allowBlank: false,
                            //fieldCls: 'required',
                            hideTrigger: true,
                            minValue: 1,
                            maxValue: 100,
                            value: 6, //預設值
                            labelAlign: 'right',
                        }, {
                            xtype: 'displayfield',
                            labelAlign: 'left',
                            labelSeparator: '',
                            value: '月',
                            width: 20
                        },
                    ]
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '年月不可空白');
                        }
                        else {
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                xtype: 'button',
                text: '匯出總表',
                id: 'T1xls',
                disabled: true,
                handler: function () {
                    if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                        Ext.Msg.alert('提醒', '年月不可空白');
                        msglabel("年月不可空白");
                    }
                    else {
                        var p = new Array();
                        p.push({ name: 'FN', value: '藥品自辦購案管制報表_總表.xls' });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });     //查詢年月     INQYM
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });   //合約期間累計結報金額 >= ?? 萬  sum_PAY_AMT
                        p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });   //核取方塊 合約期間累計結報金額>=採購上限金額 is_SELF_PUR_UPPER_LIMIT
                        p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });   //核取方塊 合約到期前 ?? 月  is_SELF_CONT_BDATE
                        p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });   //合約到期前 ?? 月  SELF_CONT_BDATE
                    };
                    PostForm(T1GetExcel, p);
                    msglabel('匯出總表完成');
                }
            }, {
                xtype: 'button',
                text: '匯出明細',
                id: 'T1Detailxls',
                disabled: true,
                handler: function () {
                    if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                        Ext.Msg.alert('提醒', '年月不可空白');
                        msglabel("年月不可空白");
                    }
                    else {
                        var p = new Array();
                        p.push({ name: 'FN', value: '藥品自辦購案管制報表_明細檔.xls' });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue }); //查詢年月     INQYM  
                        PostForm(T1GetExcel_Detail, p);
                        msglabel('匯出明細完成');
                    }
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            renderer: function (val, meta, record) {
                var MMCODE = record.data.MMCODE;
                return '<a href=javascript:void(0)>' + MMCODE + '</a>';
            },
        }, {
            text: "藥品英文名稱",
            dataIndex: 'MMNAME_E',
            width: 100
        }, {
            text: "合約案號",
            dataIndex: 'SELF_CONTRACT_NO',
            width: 100
        }, {
            text: "採購上限金額",
            dataIndex: 'SELF_PUR_UPPER_LIMIT',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "合約生效起日",
            dataIndex: 'SELF_CONT_BDATE',
            width: 100
        }, {
            text: "合約生效迄日",
            dataIndex: 'SELF_CONT_EDATE',
            width: 100
        }, {
            text: "累計結報金額",
            dataIndex: 'sum_PAY_AMT',
            style: 'text-align:left',
            width: 100, align: 'right',
            renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
        }, {
            text: "查詢年月",
            dataIndex: 'inqym',
            width: 100
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('');

                T1Rec = record;
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {

                var columnIndex = self.getHeaderCt().getHeaderAtIndex(cellIndex).config.dataIndex;
                if (columnIndex != 'MMCODE') {
                    return;
                }

                T21Load();

                detailWindow.setTitle('院內碼：' + record.data.MMCODE);

                detailWindow.show();
            },
        }
    });

    // ------ 院內碼彈出子視窗 ------

    Ext.define('T21Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'ACCOUNTDATE', type: 'string' },   //進貨日期
            { name: 'AGEN_NO', type: 'string' },       //廠商代碼   
            { name: 'MMCODE', type: 'string' },        //院內碼
            { name: 'MMNAME_E', type: 'string' },      //藥品名稱
            { name: 'M_PURUN', type: 'string' },       //單位

            { name: 'PO_PRICE', type: 'string' },      //發票單價
            { name: 'FLAG', type: 'string' },          //類別
            { name: 'DELI_QTY', type: 'string' },      //進貨量
            { name: 'PO_AMT', type: 'string' },        //發票金額
            { name: 'MEMO', type: 'string' },          //說明

            { name: 'DISC_AMT', type: 'string' },      //折讓金額
            { name: 'DISC_CPRICE', type: 'string' },   //優惠價
            { name: 'PAY_AMT', type: 'string' }        //優惠金額
        ]
    });

    var T21Store = Ext.create('Ext.data.Store', {
        model: 'T21Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0061/MMCODE_Detail',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 將MMCDOE值代入參數
                var np = {   
                    p0: T1Query.getForm().findField('P0').rawValue,   //查詢年月       
                    mmcode: T1Rec.data.MMCODE                         //院內碼
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('btnExcel').setDisabled(false);
                } else {
                    Ext.getCmp('btnExcel').setDisabled(true);
                }
            }
        }
    });

    function T21Load() {   
        T21Tool.moveFirst();
    }

    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '匯出',
                id: 'btnExcel',
                name: 'btnExcel',
                handler: function () {
                    if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                        Ext.Msg.alert('提醒', '年月不可空白');
                        msglabel("年月不可空白");
                    }
                    else {
                        var p = new Array();
                        p.push({ name: 'FN', value: '藥品自辦購案管制報表_依院內碼查詢明細.xls' });
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });   //查詢年月  INQYM
                        p.push({ name: 'mmcode', value: T1Rec.data.MMCODE});                         //院內碼
                    };
                    PostForm(T21GetExcel, p);
                    msglabel('匯出完成');
                }
            },
        ]
    });

    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight - 60,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "進貨日期",
                dataIndex: 'ACCOUNTDATE',
                width: 80
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 200
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "單位",
                dataIndex: 'M_PURUN',
                width: 40
            }, {
                text: "發票單價",
                dataIndex: 'PO_PRICE',
                width: 80,       
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "類別",
                dataIndex: 'FLAG',
                width: 60
            }, {
                text: "進貨量",
                dataIndex: 'DELI_QTY',
                width: 60
            }, {
                text: "發票金額",
                dataIndex: 'PO_AMT',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "說明",
                dataIndex: 'MEMO',
                width: 150
            }, {
                text: "折讓金額",
                dataIndex: 'DISC_AMT',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "優惠價",
                dataIndex: 'DISC_CPRICE',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "優惠金額",
                dataIndex: 'PAY_AMT',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }
        ]
    });

var detailWindow = Ext.create('Ext.window.Window', {
    renderTo: Ext.getBody(),
    modal: true,
    items: [
        {
            xtype: 'container',
            layout: 'fit',
            items: [
                {
                    xtype: 'panel',
                    itemId: 't21Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    border: false,
                    items: [T21Grid]
                }
            ],
        }
    ],
    width: "1000px",
    height: windowHeight,
    resizable: true,
    draggable: false,
    closable: false,
    y: 0,
    title: "盤點明細管理",
    buttons: [{
        text: '關閉',
        handler: function () {
            detailWindow.hide();
        }
    }],
    listeners: {
        show: function (self, eOpts) {
            detailWindow.setY(0);
        }
    }
});
detailWindow.hide();
// --------------------------

//view 
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
        items: [{
            //  xtype:'container',
            region: 'center',
            layout: {
                type: 'border',
                padding: 0
            },
            collapsible: false,
            title: '',
            split: true,
            width: '80%',
            flex: 1,
            minWidth: 50,
            minHeight: 140,
            items: [
                {
                    region: 'north',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    split: true,
                    height: '100%',
                    items: [T1Grid]
                }
            ]
        }]
    }
    ]
});
});