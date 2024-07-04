Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var reportUrl = '/Report/B/BD0023.aspx';
    var T1LastRec = null;
    
    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    var st_m_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/BD0023/GetMatClassSubCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasssubs = data.etts;
                    if (matclasssubs.length > 0) {
                        for (var i = 0; i < matclasssubs.length; i++) {
                            st_matclasssub.add({ VALUE: matclasssubs[i].VALUE, TEXT: matclasssubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });


        Ext.Ajax.request({
            url: '/api/BD0023/GetMcontidCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var m_contid = data.etts;
                    if (m_contid.length > 0) {
                        for (var i = 0; i < m_contid.length; i++) {
                            st_m_contid.add({ VALUE: m_contid[i].VALUE, TEXT: m_contid[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        

    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        resizable: true,
        autoScroll: false,
        height: 35,
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
                    xtype: 'textfield',
                    fieldLabel: '訂單編號',
                    name: 'P0',
                    enforceMaxLength: true,
                    maxLength: 15
                },
                {
                    xtype: 'combo',
                    fieldLabel: '藥材類別',
                    id: 'P1',
                    name: 'P1',
                    store: st_matclasssub,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    width: 200
                }, {
                    xtype: 'combo',
                    fieldLabel: '是否合約',
                    id: 'P2',
                    name: 'P2',
                    store: st_m_contid,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    labelWidth: 80,
                    width: 200
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });
    
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'TRANSNO', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'MEMO', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0023/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('btnReport').setDisabled(false);
                } else {
                    //msglabel('查無符合條件的資料!');
                    Ext.getCmp('btnReport').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            text: '列印全部',
            id: 'btnReport',
            name: 'btnReport',
            disabled: true,
            handler: function () {
                showReport();
            }
        }]
    });
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
                text: "訂單編號",
                dataIndex: 'PO_NO',
                style: 'text-align:left',
                align: 'left',
                width: 130
            },
            //{
            //    text: "進貨單編號",
            //    dataIndex: 'TRANSNO',
            //    style: 'text-align:left',
            //    align: 'left',
            //    width: 130
            //},
            {
                text: "入庫日期",
                dataIndex: 'INVOICE_DT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                style: 'text-align:left',
                align: 'left',
                width: 200
            }, {
                text: "備註",
                dataIndex: 'MEMO',
                style: 'text-align:left',
                align: 'left',
                width: 500
            },
            { header: "", flex: 1 }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Store.removeAll();
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    T1Form.loadRecord(records[0]);
                    T2Load();
                }
            }
        }
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'PO_PRICE', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'MEMO', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0023/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data.PO_NO,
                    p1: T1LastRec.data.INVOICE_DT
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T2Load() {
        T2Tool.moveFirst();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true
    });
    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "藥材代碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "藥材名稱",
                dataIndex: 'MMNAME_C',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "藥材包裝",
                dataIndex: 'M_PURUN',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "藥材單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "末效期",
                dataIndex: 'EXP_DATE',
                style: 'text-align:left',
                align: 'left',
                width: 80
            }, {
                text: "藥材數量",
                dataIndex: 'PO_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "單價",
                dataIndex: 'PO_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "小計",
                dataIndex: 'PO_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "發票日期",
                dataIndex: 'INVOICE_DT',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "發票號碼",
                dataIndex: 'INVOICE',
                style: 'text-align:left',
                align: 'left',
                width: 120
            },{
                text: "備註",
                dataIndex: 'MEMO',
                style: 'text-align:left',
                align: 'left',
                width: 500
            },
            { header: "", flex: 1 }
        ]
    });
    //重寫displayfield支援format屬性
    Ext.override(Ext.form.DisplayField, {
        getValue: function () {
            return this.value;
        },
        setValue: function (v) {
            this.value = v;
            this.setRawValue(this.formatValue(v));
            return this;
        },
        formatValue: function (v) {
            if (this.dateFormat && Ext.isDate(v)) {
                return v.dateFormat(this.dateFormat);
            }
            if (this.numberFormat && typeof v == 'string') {
                return Ext.util.Format.number(v, this.numberFormat);
            }
            return v;
        }
    });

    var mLabelWidth = 160;
    var mWidth = 360;
    var T1Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:0 5 0 5',
        layout: {
            type: 'table',
            columns: 1,
            border: true,
            bodyBorder: true,
            tdAttrs: { width: '25%' }
        },
        bodyPadding: '0 5 0 5',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: 160,
            width: 360,
            padding: '4 0 4 0',
            msgTarget: 'side'
        },
        defaultType: 'displayfield',// textfield
        border: false,
        items: [
            { fieldLabel: '合計', name: 'PO_AMT', numberFormat:'0,000'}
        ]
    });

    
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + T1Query.getForm().findField('P0').getValue() + '&p1=' + T1Query.getForm().findField('P1').getValue() + '&p2=' + T1Query.getForm().findField('P2').getValue() + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
            region: 'north',
            layout: 'fit',
            collapsible: false,
            title: '',
            height: '40%',
            border: false,
            items: [T1Grid]
        }, {
            itemId: 't1Form',
            region: 'north',
            collapsible: false,
            title: '',
            height: '5%',
            //  split: true,
            items: [T1Form]
        }, {
            itemId: 't2Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            height: '55%',
            split: true,
            items: [T2Grid]
        }]
    });
    
    Ext.getCmp('btnReport').setDisabled(true);
});