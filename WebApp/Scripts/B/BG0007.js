Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BG0007/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "品項採購金額統計";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0007/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_storeid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0007/GetStroeidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_contid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0007/GetContidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
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
                    xtype: 'combo',
                    fieldLabel: ' 物料分類',
                    name: 'P0',
                    id: 'P0',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '訂單日期',
                    name: 'P1',
                    id: 'P1',
                    vtype: 'dateRange',
                    dateRange: { end: 'P2' },
                    padding: '0 4 0 4',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    value: getFirstday()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'P2',
                    id: 'P2',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P1' },
                    padding: '0 4 0 4',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: 30,
                    width: 170,
                    value: getToday()
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫備分類',
                    name: 'P3',
                    id: 'P3',
                    store: st_storeid,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: 160
                }, {
                    xtype: 'combo',
                    fieldLabel: '合約分類',
                    name: 'P4',
                    id: 'P4',
                    store: st_contid,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: 160
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '品項累計金額超過',
                    name: 'P5',
                    id: 'P5',
                    enforceMaxLength: true,
                    maxLength: 10,
                    labelWidth: 130,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null) ||
                            (this.up('form').getForm().findField('P1').getValue() == '' || this.up('form').getForm().findField('P1').getValue() == null) ||
                            (this.up('form').getForm().findField('P2').getValue() == '' || this.up('form').getForm().findField('P2').getValue() == null) ||
                            (this.up('form').getForm().findField('P3').getValue() == '' || this.up('form').getForm().findField('P3').getValue() == null) ||
                            (this.up('form').getForm().findField('P4').getValue() == '' || this.up('form').getForm().findField('P4').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '物料分類、訂單日期、庫備分類及合約分類不可空白');
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

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function getFirstday() {
        var date = new Date(), y = date.getFullYear(), m = date.getMonth();
        var firstDay = new Date(y, m, 1);
        var lastDay = new Date(y, m + 1, 0);
        return firstDay
    }
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'PO_PRICE', type: 'float' },
            { name: 'PO_QTY', type: 'float' },
            { name: 'PO_AMT', type: 'float' },
            { name: 'TOTSUM', type: 'float' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }, { property: 'PO_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0007/All',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        
    }
    
    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
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
        }
        ],
        //selModel: {
        //    checkOnly: false,
        //    allowDeselect: true,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 120,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 180
        }, {
            text: "包裝單位",
            dataIndex: 'M_PURUN',
            width: 70
        }, {
            text: "合約單價",
            dataIndex: 'PO_PRICE',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "訂單數量",
            dataIndex: 'PO_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "單筆總價",
            dataIndex: 'PO_AMT',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "累計金額",
            dataIndex: 'TOTSUM',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
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
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P0').setValue('02');
    T1Query.getForm().findField('P3').setValue('0');
    T1Query.getForm().findField('P4').setValue('0');
});
