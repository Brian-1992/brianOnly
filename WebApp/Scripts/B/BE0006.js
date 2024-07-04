Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1LastRec = null;
    var T1GetExcel = '/api/BE0006/GetAllExcel';   //資料匯出
    //var T1Get = '/api/BE0006/GetAll';

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0006/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var MatClassCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('P0');
                if (MatClassCount > 0) {
                    combo_P0.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });


    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },         // 訂單號碼
            { name: 'M_CONTID', type: 'string' },     // 合約類別
            { name: 'AGEN_NAME', type: 'string' },    // 廠商名稱
            { name: 'INVOICE', type: 'string' },      // 發票號碼
            { name: 'INVOICE_DT', type: 'string' },   // 發票日期
            { name: 'EMAIL_DT', type: 'string' },     // 寄EMAIL日期
            { name: 'REPLY_DT', type: 'string' },     // 收信確認日期
            { name: 'MMCODE', type: 'string' },       // 院內碼
            { name: 'MMNAME', type: 'string' },       // 中文名稱
            { name: 'MMNAME_E', type: 'string' },     // 英文名稱
            { name: 'PO_PRICE', type: 'string' },     // 合約價
            { name: 'PO_QTY', type: 'string' },       // 訂單數量
            { name: 'AMOUNT', type: 'string' },       // 單筆金額
            { name: 'DELI_DT', type: 'string' },      // 進貨日期
            { name: 'PO_TIME', type: 'string' },      // 訂單日期
            { name: 'TWN_PO_TIME', type: 'string' },  // 訂單日期
            { name: 'agen_email ', type: 'string' },  // 廠商email            
            { name: 'TRANSNO', type: 'string' },      // PH_INVOICE的資料流水號
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'StartDate', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0006/GetAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    P1: T1Query.getForm().findField('P1').getRawValue(),
                    P2: T1Query.getForm().findField('P2').getRawValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    // 查詢欄位
    var mLabelWidth = 70;
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
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    bodyStyle: 'padding: 3px 5px;',
                    items: [
                         {
                            xtype: 'combo',
                            fieldLabel: '物料分類',
                            store: st_MatClass,
                            name: 'P0',
                            id: 'P0',
                            labelWidth: 80,
                            width: 240,
                            queryMode: 'local',
                            fieldCls: 'required',
                            allowBlank: false,
                            displayField: 'COMBITEM',
                            valueField: 'VALUE'                                             
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '進貨日期',
                            name: 'P1',
                            id: 'P1',
                            enforceMaxLength: true,
                            maxLength: 7,
                            labelWidth: 80,
                            width: 160,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',                            
                            value: getDay(-Ext.Date.getDayOfYear(new Date()),'-'),                            
                            regexText: '請選擇日期'
                        },{
                            xtype: 'datefield',
                            fieldLabel: '至',
                            labelSeparator: '',
                            name: 'P2',
                            id: 'P2',
                            enforceMaxLength: true,
                            maxLength: 7,
                            labelWidth: mLabelWidth,
                            width: 88,
                            labelWidth: 8,
                            padding: '0 2 0 2',
                            allowBlank: false,
                            fieldCls: 'required',
                            value: new Date(),
                            regexText: '請選擇日期'
                        },{
                            xtype: 'button',
                            text: '刷新',                            
                            handler: function () {                            
                                msglabel('訊息區:');
                                if (T1Query.getForm().isValid()) {                                                
                                    T1Load();
                                } else {
                                    Ext.Msg.alert('提醒', '刷新條件要輸入完整');
                                }
                            }              
                        }
                    ]
                },
            ]
        }]
    });

    //正數為後一天，負數為前一天，str為日期間隔符，例如 -,/ 
    function getDay(num, str) {
        var today = new Date();
        var nowTime = today.getTime();
        var ms = 24 * 3600 * 1000 * num;
        today.setTime(parseInt(nowTime + ms));
        var oYear = today.getFullYear();
        var oMoth = (today.getMonth() + 1).toString();
        if (oMoth.length <= 1)
            oMoth = '0' + oMoth;
        var oDay = today.getDate().toString();
        if (oDay.length <= 1)
            oDay = '0' + oDay;
        return oYear + str + oMoth + str + oDay;
    }

    function sendMail() {
        Ext.Ajax.request({
            url: '/api/BE0006/UpdateInvoiceEmail',
            method: reqVal_p,
            params: {
                P0: T1Query.getForm().findField('P0').getValue(),
                P1: T1Query.getForm().findField('P1').getRawValue(),
                P2: T1Query.getForm().findField('P2').getRawValue(),
            },
            success: function (response) {
                msglabel('訊息區:寄送MAIL完成');
            },
            failure: function (response, options) {
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemID: 'SendMail',
                text: '寄送MAIL',
                handler: function () {
                    sendMail();
                    //setFormT1("U", '修改');
                }
            }, {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '發票缺漏報表.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });    // 物料分類 
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getRawValue() });    // 進貨日期（起）
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getRawValue() });    // 進貨日期（迄）

                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                },
            }]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "訂單號碼",
                dataIndex: 'PO_NO',
                width: 120,
                renderer: function (value, metaData, record, rowIndex) {
                    if ((record.data.agen_email == '') || (record.data.agen_email == null)) {
                        metaData.tdStyle = 'background-color:#ffaaaa'; //'<span>' + value + '</span>';
                    }
                    return value;
                }
            }, {
                text: "合約類別",
                dataIndex: 'M_CONTID',
                width: 80
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAME',
                width: 180
            }, {
                text: "發票號碼",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'INVOICE',
                width: 90
            }, {
                text: "發票日期",
                dataIndex: 'INVOICE_DT,',
                width: 75
            }, {
                text: "寄EMAIL日期",
                dataIndex: 'EMAIL_DT',
                width: 120
            }, {
                text: "收信確認日期",
                dataIndex: 'REPLY_DT',
                width: 120
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            }, {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 180
            }, {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 180
            }, {
                text: "合約價",
                dataIndex: 'PO_PRICE',
                width: 110
            }, {
                text: "訂單數量",
                dataIndex: 'PO_QTY',
                width: 110
            }, {
                text: "進貨數量",
                dataIndex: 'DELI_QTY',
                width: 110
            }, {
                text: "單筆金額",
                dataIndex: 'AMOUNT',
                width: 110
            }, {
                text: "進貨日期",
                dataIndex: 'DELI_DT',
                width: 110
            }, {
                text: "訂單日期",
                dataIndex: 'TWN_PO_TIME',
                width: 110
            }, {
                text: "廠商email",
                dataIndex: 'agen_email',
                width: 200,
                renderer: function (value, metaData, record, rowIndex) {
                    if ((record.data.agen_email == '') || (record.data.agen_email == null)) {
                        metaData.tdStyle = 'background-color:#ffaaaa'; //'<span>' + value + '</span>';
                    }
                    return value;
                }
            }, {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');
                T1LastRec = record;
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
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '100%',
                        split: true,
                        items: [T1Grid]
                    },
                ]
            }
        ]
    });
});