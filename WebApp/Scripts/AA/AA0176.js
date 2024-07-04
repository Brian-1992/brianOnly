Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Name = "批次進貨";
    var T1Set = '/api/AA0176/DetailUpdate';
    var qtySet = '/api/AA0176/SetQty';
    var priceSet = '/api/AA0176/SetPrice';
    var transSet = '/api/AA0176/SetTrans';
    var transBackSet = '/api/AA0176/SetTransBack';
    var splitMmcode = '/api/AA0176/SplitMmcode';
    var impInvoice = '/api/AA0176/ImportInvoice';
    var ImportGetExcel = '/api/AA0176/Excel';
    var amtGet = '/api/AA0176/calcAmtMsg';

    var T1Rec = 0;
    var T1LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;

    var T11Rec = 0;
    var T11LastRec = null;
    var T21Rec = 0;
    var T21LastRec = null;

    var hosp_code = '';


    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    function get_hosp_code() {
        //hosp_code
        Ext.Ajax.request({
            url: '/api/AA0176/GetLoginInfo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var LoginInfo = data.etts;
                    if (LoginInfo.length > 0) {
                        for (var i = 0; i < LoginInfo.length; i++) {
                            if (LoginInfo[i].HOSP_CODE == '803') {
                                T1Grid.reconfigure(T1Store, [
                                    {
                                        xtype: 'rownumberer'
                                    }, {
                                        text: "訂單號碼",
                                        dataIndex: 'PO_NO',
                                        width: 130,
                                        sortable: true,
                                        renderer: function (val, meta, record) {
                                            if (record.data['isEdit'] == 'Y') // 若訂單進貨資料有維護過,則以藍字顯示
                                                return '<font color=blue>' + val + '</font>';
                                            else
                                                return val;
                                        }
                                    }, {
                                        text: "廠商簡稱",
                                        dataIndex: 'EASYNAME',
                                        width: 100,
                                        sortable: true
                                    },{
                                        text: "廠商代碼",
                                        dataIndex: 'AGEN_NO',
                                        width: 80,
                                        sortable: true
                                    }, {
                                        text: "廠商名稱",
                                        dataIndex: 'AGEN_NAMEC',
                                        width: 250,
                                        sortable: true
                                    }, {
                                        text: "訂單日期",
                                        dataIndex: 'PO_TIME',
                                        width: 80,
                                        sortable: true
                                    }, {
                                        header: "",
                                        flex: 1
                                    }
                                ]);
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    get_hosp_code();
    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0176/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var MatClassCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('P0');
                var combo_T21P0 = T2Query.getForm().findField('T21P0');
                if (MatClassCount > 0) {
                    combo_P0.setValue(store.getAt(0).get('VALUE'));
                    combo_T21P0.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });

    var T11QueryAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'P5',
        id: 'P5',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: '/api/AA0176/GetAgennoCombo',
        width: 200,
        matchFieldWidth: false,
        listConfig: { width: 300 }
    });
    var T21QueryAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'T21P5',
        id: 'T21P5',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: '/api/AA0176/GetAgennoCombo',
        width: 170,
        matchFieldWidth: false,
        listConfig: { width: 300 }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        resizable: true,
        autoScroll: false,
        bodyPadding: '0 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'panel',
                id: 'PanelT1P1',
                border: false,
                layout: 'vbox',
                autoScroll: true,
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料類別',
                        name: 'P0',
                        id: 'P0',
                        width: 200,
                        matchFieldWidth: false,
                        listConfig: { width: 170 },
                        store: st_matclass,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '訂單日期',
                        name: 'P3',
                        id: 'P3',
                        width: 150,
                        fieldCls: 'required',
                        allowBlank: false,
                        vtype: 'dateRange',
                        dateRange: { end: 'P4' },
                        padding: '0 4 0 4',
                        value: new Date()
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: '10px',
                        name: 'P4',
                        id: 'P4',
                        width: 100,
                        fieldCls: 'required',
                        allowBlank: false,
                        labelWidth: 20,
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'P3' },
                        padding: '0 4 0 54',
                        value: new Date()
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '訂單號碼',
                        name: 'P1',
                        id: 'P1',
                        width: 200
                    }, T11QueryAgenno, {
                        xtype: 'textfield',
                        fieldLabel: '廠商簡稱',
                        name: 'P7',
                        id: 'P7',
                        width: 200
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'P6',
                        id: 'P6',
                        width: 200
                    },
                    {
                        xtype: 'checkbox',
                        name: 'P8',
                        id: 'P8',
                        width: 190,
                        boxLabel: '只顯示已維護訂單(藍字)',
                        inputValue: 'Y',
                        checked: false,
                        padding: '0 4 0 8'
                    },
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        autoScroll: true,
                        items: [
                            {
                                xtype: 'button',
                                itemId: 'query', text: '查詢',
                                handler: function () {
                                    msglabel('訊息區:');
                                    if (chkT1QueryValid())
                                        T1Load();
                                }
                            }, {
                                xtype: 'button',
                                itemId: 'clean', text: '清除', handler: function () {
                                    var f = T1Query.getForm();
                                    f.reset();
                                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                    msglabel('訊息區:');
                                }
                            }, {
                                xtype: 'button',
                                itemId: 'ipt', text: '匯入', margin: '0 0 0 12', handler: function () {
                                    showWinImport();
                                    msglabel('訊息區:');
                                }
                            }
                        ]
                    }

                ]
            }
        ]
    });

    var T11Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        resizable: true,
        autoScroll: false,
        height: 65,
        bodyPadding: '0 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'panel',
                id: 'PanelT11P1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'P2',
                        id: 'P2',
                        width: 150
                    }, {
                        xtype: 'checkbox',
                        name: 'P6',
                        width: 130,
                        boxLabel: '顯示細項金額',
                        inputValue: 'Y',
                        checked: false,
                        padding: '0 4 0 8',
                        listeners:
                        {
                            change: function (rg, nVal, oVal, eOpts) {
                                setVisibleColumns(nVal);
                            }
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'query', text: '查詢',
                        handler: function () {
                            msglabel('訊息區:');
                            T11Load();
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'clean', text: '清除', handler: function () {
                            var f = T11Query.getForm();
                            f.reset();
                            msglabel('訊息區:');
                        }
                    }
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelT11P2',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'label',
                        text: '',
                        padding: '4 4 0 20',
                        name: 'T11Query_AmtMsg',
                        id: 'T11Query_AmtMsg',
                        width: 1200
                    }
                ]
            }
        ]
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        resizable: true,
        autoScroll: false,
        height: 35,
        bodyPadding: '0 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'panel',
                id: 'PanelT2P1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料類別',
                        name: 'T21P0',
                        id: 'T21P0',
                        width: 200,
                        matchFieldWidth: false,
                        listConfig: { width: 170 },
                        store: st_matclass,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '訂單號碼',
                        name: 'T21P1',
                        id: 'T21P1',
                        width: 180
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '訂單日期',
                        name: 'T21P10',
                        id: 'T21P10',
                        width: 150,
                        fieldCls: 'required',
                        allowBlank: false,
                        vtype: 'dateRange',
                        dateRange: { end: 'P4' },
                        padding: '0 4 0 4',
                        value: new Date()
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: '10px',
                        name: 'T21P11',
                        id: 'T21P11',
                        width: 100,
                        fieldCls: 'required',
                        allowBlank: false,
                        labelWidth: 20,
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'T21P10' },
                        value: new Date()
                    }, T21QueryAgenno,
                    {
                        xtype: 'button',
                        itemId: 'query', text: '查詢',
                        handler: function () {
                            msglabel('訊息區:');
                            if (chkT2QueryValid()) {
                                T2Load();
                            }
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'clean', text: '清除', handler: function () {
                            var f = T2Query.getForm();
                            f.reset();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]
    });

    var T21Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        resizable: true,
        autoScroll: false,
        bodyPadding: '0 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'panel',
                id: 'PanelT21P1',
                border: false,
                layout: 'hbox',
                padding: '0 0 5 0',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'T21P2',
                        id: 'T21P2',
                        width: 200
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '進貨日期',
                        name: 'T21P3',
                        id: 'T21P3',
                        width: 150,
                        vtype: 'dateRange',
                        dateRange: { end: 'T21P4' },
                        padding: '0 4 0 4',
                        // value: new Date()
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: '10px',
                        name: 'T21P4',
                        id: 'T21P4',
                        width: 100,
                        labelWidth: 20,
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'T21P3' },
                        value: new Date()
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '批號',
                        name: 'T21P7',
                        id: 'T21P7',
                        labelWidth: 50,
                        width: 200
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '效期',
                        name: 'T21P8',
                        id: 'T21P8',
                        labelWidth: 50,
                        width: 130,
                        vtype: 'dateRange',
                        dateRange: { end: 'T21P9' },
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: '10px',
                        name: 'T21P9',
                        id: 'T21P9',
                        width: 100,
                        labelWidth: 20,
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'T21P8' }
                    }
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelT21P2',
                border: false,
                layout: 'hbox',
                padding: '5 0 0 0',
                items: [
                    {
                        xtype: 'checkbox',
                        name: 'T21P12',
                        width: 150,
                        boxLabel: '顯示已還原為未進貨項目',
                        inputValue: 'Y',
                        checked: false,
                        padding: '0 4 0 8'
                    }, {
                        xtype: 'checkbox',
                        name: 'T21P6',
                        width: 140,
                        boxLabel: '顯示還原為未進貨記錄',
                        inputValue: 'Y',
                        checked: false,
                        padding: '0 4 0 8'
                    }, {
                        xtype: 'button',
                        itemId: 'query', text: '查詢',
                        handler: function () {
                            msglabel('訊息區:');
                            T21Load();
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'clean', text: '清除', handler: function () {
                            var f = T21Query.getForm();
                            f.reset();
                            f.findField('T21P0').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'PO_TIME', type: 'string' },
            { name: 'isEdit', type: 'string' }
        ]
    });
    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'MAT_CLSNAME', type: 'string' },
            { name: 'PO_TIME', type: 'string' },
            { name: 'POACC_SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'STORE_LOC', type: 'string' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'DELI_QTY', type: 'string' },
            { name: 'DELI_QTY_SUM', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'UNIT_SWAP', type: 'string' },
            { name: 'PO_PRICE', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'M_DISCPERC', type: 'string' },
            { name: 'M_DISCPERC_M', type: 'string' },
            { name: 'UPRICE', type: 'string' },
            { name: 'UPRICE_M', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'DISC_CPRICE_M', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'DISC_UPRICE_M', type: 'string' },
            { name: 'INQTY', type: 'string' },
            { name: 'ACC_QTY', type: 'string' },
            { name: 'ACC_AMT', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'EXTRA_DISC_AMOUNT', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'DELI_STATUS', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' }
        ]
    });
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' }
        ]
    });
    Ext.define('T21Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'BW_SQTY', type: 'string' },
            { name: 'ACC_PO_PRICE', type: 'string' },
            { name: 'PO_PRICE_AMT', type: 'string' },
            { name: 'ACC_DISC_CPRICE', type: 'string' },
            { name: 'ACC_QTY', type: 'string' },
            { name: 'CFM_QTY', type: 'string' },
            { name: 'ACC_BASEUNIT', type: 'string' },
            { name: 'STATUS', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'ACC_TIME', type: 'string' },
            { name: 'ACC_USER', type: 'string' },
            { name: 'STOREID', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'ACC_PURUN', type: 'string' },
            { name: 'UNIT_SWAP', type: 'string' },
            { name: 'WEXP_ID', type: 'string' },
            { name: 'TX_QTY_T', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'EXTRA_DISC_AMOUNT', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'SRC_SEQ', type: 'string' },
            { name: 'REF_CNT', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0176/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 在store load之前也加入檢查,以避免查詢條件不合法時透過重新排序的方式進行查詢
                if (chkT1QueryValid()) {
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').getValue(),
                        p3: T1Query.getForm().findField('P3').rawValue,
                        p4: T1Query.getForm().findField('P4').rawValue,
                        p5: T1Query.getForm().findField('P5').getValue(),
                        p6: T1Query.getForm().findField('P6').getValue(),
                        p7: T1Query.getForm().findField('P7').getValue(),
                        p8: T1Query.getForm().findField('P8').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else
                    return false;
                T11Query.down('#T11Query_AmtMsg').setText('');
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

    var T11Store = Ext.create('Ext.data.Store', {
        model: 'T11Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DELI_STATUS', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0176/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                if (T1LastRec) {
                    var np = {
                        p1: T1LastRec.data.PO_NO,
                        p2: T11Query.getForm().findField('P2').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else {
                    T11Store.removeAll();
                    return false;
                }
            },
            load: function (store, options) {
                //T11Grid.down('#edit').setDisabled(true);
                T11Cleanup();
                T11Grid.down('#add').setDisabled(true);
                T11Grid.down('#splitBtn').setDisabled(true);
                T11Grid.down('#delBtn').setDisabled(true);
                T11Grid.down('#importInvoice').setDisabled(true);
                T11Grid.down('#completeBtn').setDisabled(true);

                if (T1LastRec)
                    calAmtMsg(T1LastRec.data.PO_NO);
                else
                    T11Query.down('#T11Query_AmtMsg').setText('');
            }
        }
    });
    function T11Load() {
        T11Store.load({
            params: {
                start: 0
            }
        });
        setFormT11a();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        setFormT11("U", '修改');
            //    }
            //},
            {
                itemId: 'add', text: '進貨', handler: function () {
                    var selection = T11Grid.getSelection();
                    var poacc_seq = '';
                    var chkResult = '';
                    var chkResultMsg1 = '';
                    var chkResultMsg2 = '';
                    var chkResultMsg3 = '';
                    var sumAccAmt = 0;
                    if (selection.length) {
                        $.map(selection, function (item, key) {
                            if (item.get('INQTY') > 0)
                                poacc_seq += item.get('POACC_SEQ') + '^';
                        })
                    }

                    if (selection.length == 0) {
                        Ext.Msg.alert('訊息', '尚未選擇任何進貨資料，請確認!');
                        return;
                    }
                    else if (poacc_seq == '') {
                        Ext.Msg.alert('訊息', '選擇的資料尚未填寫本次進貨量，請確認!');
                        return;
                    }
                    else {
                        var INQTY_SUMlist = [];
                        for (var i = 0; i < selection.length; i++) {
                            if (selection[i].data.INQTY > 0) {
                                if (selection[i].data.DELI_STATUS == 'Y') {
                                    Ext.Msg.alert('錯誤', selection[i].data.MMCODE + '已進貨完成或設定為不再進貨,不可再進貨.');
                                    return;
                                }
                                // 本次進貨量,小計 編輯欄位是否都有填寫
                                else if (selection[i].data.INQTY == '' || selection[i].data.ACC_AMT == '') {
                                    Ext.Msg.alert('錯誤', selection[i].data.MMCODE + ' ' + selection[i].data.MEMO + ' 進貨量,小計未填寫完成，請重新確認');
                                    return;
                                }
                                // 總進貨量是否大於訂貨量(只提示，若不通過仍可確認進貨)
                                else if (Number(selection[i].data.PO_QTY) < Number(selection[i].data.DELI_QTY_SUM) + Number(selection[i].data.INQTY)) {
                                    chkResult = '1';
                                    chkResultMsg1 += selection[i].data.MMCODE + ' 備註:' + selection[i].data.MEMO + '<br>'
                                }

                                // 檢查勾選項目的進貨量總和是否大於已進貨量(只提示，若不通過仍可確認進貨)
                                var chkExist = false;
                                for (var j = 0; j < INQTY_SUMlist.length; j++) {
                                    if (INQTY_SUMlist[j].MMCODE == selection[i].data.MMCODE) {
                                        INQTY_SUMlist[j].INQTY += Number(selection[i].data.INQTY);
                                        chkExist = true;
                                        if (Number(selection[i].data.PO_QTY) < Number(selection[i].data.DELI_QTY_SUM) + Number(INQTY_SUMlist[j].INQTY)) {
                                            chkResult = '2';
                                            chkResultMsg2 += selection[i].data.MMCODE + ' 備註:' + selection[i].data.MEMO + '<br>'
                                        }
                                    }
                                }
                                if (chkExist == false) {
                                    var item = {
                                        MMCODE: selection[i].data.MMCODE,
                                        INQTY: Number(selection[i].data.INQTY)
                                    }
                                    INQTY_SUMlist.push(item);
                                }
                                sumAccAmt += Number(selection[i].data.ACC_AMT);

                                // 檢查檢查批號、效期、發票日期、發票號碼均未填寫的項目
                                if (selection[i].data.LOT_NO == '' && selection[i].data.EXP_DATE == ''
                                    && selection[i].data.INVOICE == '' && selection[i].data.INVOICE_DT == '') {
                                    chkResultMsg3 += selection[i].data.MMCODE + ' 備註:' + selection[i].data.MEMO + '<br>'
                                }

                            }
                        }
                    }
                    // 若有檢查批號、效期、發票日期、發票號碼均未填寫的項目,增加提示訊息
                    var allNotFillNotice = '';
                    if (chkResultMsg3) {
                        allNotFillNotice = '<font color=red>' + chkResultMsg3 + '批號、效期、發票日期、發票號碼均未填寫，</font><br>'
                    }

                    var confirmMsg = allNotFillNotice + '本次進貨小計' + sumAccAmt + '，是否確定進貨?';
                    if (chkResult == '1')
                        confirmMsg = '院內碼<br>' + chkResultMsg1 + ' 進貨量加總大於待進貨量，是否確定進貨?';
                    else if (chkResult == '2')
                        confirmMsg = '院內碼<br>' + chkResultMsg2 + '勾選項目的進貨量加總大於待進貨量，是否確定進貨?';
                    Ext.MessageBox.confirm('進貨', confirmMsg, function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();

                            Ext.Ajax.request({
                                url: qtySet,
                                method: reqVal_p,
                                params: {
                                    PO_NO: T1LastRec.data.PO_NO,
                                    POACC_SEQ: poacc_seq
                                },
                                success: function (response) {
                                    myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel("進貨完成");
                                        if (data.msg == 'DONE') // 若本次進貨後此單全部品項都已進完則重新查詢Master, 否則查詢Detail
                                            T1Load();
                                        else
                                            T11Load();
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {
                                    myMask.hide();
                                }
                            });
                        }
                    }
                    );
                }
            },
            {
                itemId: 'splitBtn', text: '拆項', disabled: true, handler: function () {
                    popSplitFormWin(T11LastRec.data.MMCODE, T11LastRec.data.PO_QTY, T11LastRec.data.DELI_QTY);
                }
            },
            {
                itemId: 'delBtn', text: '刪除', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除? <br>' + T11LastRec.data.MMCODE + '<br>' + T11LastRec.data.MEMO, function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0176/DetailDelete',
                                method: reqVal_p,
                                params: {
                                    PO_NO: T11LastRec.data.PO_NO,
                                    POACC_SEQ: T11LastRec.data.POACC_SEQ
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('訊息區:資料刪除成功');

                                        T11Load();
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            },
            {
                itemId: 'importInvoice', text: '批次填入', disabled: true, handler: function () {
                    // 2024.01.12桃園希望可以批次填入折讓金額,連帶批號效期,備註也一併可批次填入
                    popInvoiceFormWin();
                }
            },
            {
                itemId: 'completeBtn', text: '不再進貨', disabled: true, handler: function () {
                    if (T11LastRec.data.DELI_STATUS != 'Y') {
                        Ext.MessageBox.confirm('不再進貨', '是否確定此項目不再進貨,設定為進貨完成? <br>' + T11LastRec.data.MMCODE + '<br>' + T11LastRec.data.MEMO, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0176/DetailComplete',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: T11LastRec.data.PO_NO,
                                        POACC_SEQ: T11LastRec.data.POACC_SEQ
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料設定成功');

                                            if (data.msg == 'DONE') // 設定後全部品項都已進完則重新查詢Master, 否則查詢Detail
                                                T1Load();
                                            else
                                                T11Load();
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('訊息', '此項目已設定為進貨完成');
                    }
                }
            }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'DESC' }
        ],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0176/AllAccLogM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 在store load之前也加入檢查,以避免查詢條件不合法時透過重新排序的方式進行查詢
                if (chkT2QueryValid()) {
                    var np = {
                        p0: T2Query.getForm().findField('T21P0').getValue(),
                        p1: T2Query.getForm().findField('T21P1').getValue(),
                        p5: T2Query.getForm().findField('T21P5').getValue(),
                        p10: T2Query.getForm().findField('T21P10').rawValue,
                        p11: T2Query.getForm().findField('T21P11').rawValue
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else
                    return false;
            }
        }
    });
    function T2Load() {
        T2Tool.moveFirst();
    }

    var T21Store = Ext.create('Ext.data.Store', {
        model: 'T21Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SRC_SEQ', direction: 'DESC' },
        { property: 'REF_CNT', direction: 'DESC' },
        { property: 'SEQ', direction: 'DESC' }
        ],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0176/AllAccLogD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                if (T2LastRec) {
                    var np = {
                        p1: T2LastRec.data.PO_NO,
                        p2: T21Query.getForm().findField('T21P2').getValue(),
                        p3: T21Query.getForm().findField('T21P3').rawValue,
                        p4: T21Query.getForm().findField('T21P4').rawValue,
                        p6: T21Query.getForm().findField('T21P6').getValue(),
                        p12: T21Query.getForm().findField('T21P12').getValue(),
                        p7: T21Query.getForm().findField('T21P7').getValue(), //批號
                        p8: T21Query.getForm().findField('T21P8').rawValue,   //效期起
                        p9: T21Query.getForm().findField('T21P9').rawValue    //效期迄
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else {
                    T21Store.removeAll();
                    return false;
                }
            },
            load: function (store, options) {
                T21Grid.down('#trans_back').setDisabled(true);
            }
        }
    });
    function T21Load() {
        T21Store.load({
            params: {
                start: 0
            }
        });
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'trans_save', text: '儲存', handler: function () {
                    var store = T21Grid.getStore().data.items;

                    var list = [];
                    for (var i = 0; i < store.length; i++) {
                        var item = {
                            SEQ: store[i].data.SEQ,
                            PO_NO: store[i].data.PO_NO,
                            MMCODE: store[i].data.MMCODE,
                            ACC_AMT: store[i].data.ACC_AMT,
                            INVOICE: store[i].data.INVOICE,
                            INVOICE_DT: Ext.util.Format.date(store[i].data.INVOICE_DT, 'Xmd'),
                            MEMO: store[i].data.MEMO
                        }
                        list.push(item);
                    }
                    if (list.length == 0) {
                        Ext.Msg.alert('訊息', '沒有可更新的進貨資料!');
                        return;
                    }
                    Ext.Ajax.request({
                        url: transSet,
                        method: reqVal_p,
                        params: {
                            list: Ext.util.JSON.encode(list)
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel("儲存完成");
                                T21Load();
                            } else {
                                Ext.MessageBox.alert('錯誤', data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            }, {
                itemId: 'trans_back', text: '還原為未進貨', disabled: true, handler: function () {
                    Ext.MessageBox.confirm('還原為未進貨', T21LastRec.data.PO_NO + ' ' + T21LastRec.data.MMCODE + '是否確定還原為未進貨?', function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            Ext.Ajax.request({
                                url: transBackSet,
                                method: reqVal_p,
                                params: {
                                    PO_NO: T21LastRec.data.PO_NO,
                                    MMCODE: T21LastRec.data.MMCODE,
                                    SEQ: T21LastRec.data.SEQ
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        myMask.hide();
                                        msglabel('訊息區:還原為未進貨成功');
                                        T21Load();
                                    }
                                    else {
                                        myMask.hide();
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                        msglabel('訊息區:' + data.msg);
                                    }
                                },
                                failure: function (response) {
                                    myMask.hide();
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        ]
    });

    function setFormT11(x, t) {
        // viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        u = f.findField('INQTY');
        f.findField('x').setValue(x);
        f.findField('INQTY').setReadOnly(false);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('INVOICE').setReadOnly(false);
        f.findField('INVOICE_DT').setReadOnly(false);
        f.findField('STORE_LOC').setReadOnly(false);
        f.findField('EXTRA_DISC_AMOUNT').setReadOnly(false);
        f.findField('PO_PRICE').setReadOnly(false);
        //f.findField('DISC_CPRICE').setReadOnly(false);
        f.findField('ACC_AMT').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    function setFormT11a() {
        if (T11LastRec) {
            T1Form.loadRecord(T11LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            viewport.down('#form').expand();

            // 總訂購量大於1才可拆項
            if (T11LastRec.data.PO_QTY > 1 && T11Grid.getSelection().length == 1)
                T11Grid.down('#splitBtn').setDisabled(false);
            else
                T11Grid.down('#splitBtn').setDisabled(true);

            calAmt(false);
        }
        else {
            T1Form.getForm().reset();
            viewport.down('#form').collapse();
            //T11Grid.down('#edit').setDisabled(true);
            T11Grid.down('#add').setDisabled(true);
            T11Grid.down('#splitBtn').setDisabled(true);
            T11Grid.down('#delBtn').setDisabled(true);
            T11Grid.down('#importInvoice').setDisabled(true);
            T11Grid.down('#completeBtn').setDisabled(true);
        }
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query],
	        resizable: true
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 130,
            sortable: true,
            renderer: function (val, meta, record) {
                if (record.data['isEdit'] == 'Y') // 若訂單進貨資料有維護過,則以藍字顯示
                    return '<font color=blue>' + val + '</font>';
                else
                    return val;
            }
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO',
            width: 80,
            sortable: true
        }, {
            text: "廠商簡稱",
            dataIndex: 'EASYNAME',
            width: 100,
            sortable: true
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 250,
            sortable: true
        }, {
            text: "訂單日期",
            dataIndex: 'PO_TIME',
            width: 80,
            sortable: true
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                T11Load();
            }
        }
    });

    var checkboxT11Model = Ext.create('Ext.selection.CheckboxModel', {
        listeners: {
            'beforeselect': function (view, rec, index) {

            },
            'select': function (view, rec) {
                T11ChkBtns();
            },
            'deselect': function (view, rec) {
                T11ChkBtns();
            }
        }
    });

    var T11Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T11Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T11Query],
            resizable: true
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }
        ],
        selModel: checkboxT11Model,
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: '',
            width: 90,
            renderer: function (val, meta, record) {
                if (record.data['PO_PRICE'] != record.data['M_CONTPRICE'])
                    return '<a href=javascript:chgPrice("' + record.data['PO_NO'] + '","' + record.data['MMCODE'] + '","' + record.data['PO_PRICE'] + '","' + record.data['M_CONTPRICE'] + '") >變更單價</a>';
                else
                    return '';
            }
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true,
            renderer: function (val, meta, record) {
                // 已進貨完成標示
                if (record.data["DELI_STATUS"] == 'Y') {
                    return '<font color="blue">' + val + '</font>';
                }
                else {
                    return '<font color="black">' + val + '</font>';
                }
            }
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 300,
            sortable: true,
            renderer: function (val, meta, record) {
                // 已進貨完成標示
                if (record.data["DELI_STATUS"] == 'Y') {
                    return '<font color="blue">' + val + '</font>';
                }
                else {
                    return '<font color="black">' + val + '</font>';
                }
            }
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 100,
            sortable: true,
            renderer: function (val, meta, record) {
                // 已進貨完成標示
                if (record.data["DELI_STATUS"] == 'Y') {
                    return '<font color="blue">' + val + '</font>';
                }
                else {
                    return '<font color="black">' + val + '</font>';
                }
            }
        }, {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            style: 'text-align:left',
            width: 100,
            align: 'left'
        }, {
            text: "總訂購量",
            dataIndex: 'PO_QTY',
            style: 'text-align:left',
            width: 80, align: 'right',
            renderer: function (val, meta, record) {
                // 已進貨完成標示
                if (record.data["DELI_STATUS"] == 'Y') {
                    return '<font color="blue">' + val + '</font>';
                }
                else {
                    return '<font color="black">' + val + '</font>';
                }
            }
        }, {
            text: "已進貨量",
            dataIndex: 'DELI_QTY',
            style: 'text-align:left',
            width: 80, align: 'right',
            renderer: function (val, meta, record) {
                // 已進貨完成標示
                if (record.data["DELI_STATUS"] == 'Y') {
                    return '<font color="blue">' + val + '</font>';
                }
                else {
                    return '<font color="black">' + val + '</font>';
                }
            }
        }, {
            text: "申購計量單位",
            dataIndex: 'M_PURUN',
            width: 120,
            sortable: true
        }, {
            text: "訂單單價",   //1121208改回 訂單單價以免造成誤解
            dataIndex: 'PO_PRICE',
            style: 'text-align:left',
            width: 90,
            align: 'right'
        }, {
            text: "現行單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "訂單折讓比",
            dataIndex: 'M_DISCPERC',
            style: 'text-align:left',
            width: 110,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行折讓比",
            dataIndex: 'M_DISCPERC_M',
            style: 'text-align:left',
            width: 110,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "訂單最小單價",
            dataIndex: 'UPRICE',
            style: 'text-align:left',
            width: 130,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行最小單價",
            dataIndex: 'UPRICE_M',
            style: 'text-align:left',
            width: 130,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "優惠單價", // 訂單優惠合約單價
            dataIndex: 'DISC_CPRICE',
            style: 'text-align:left',
            width: 130,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行優惠單價",
            dataIndex: 'DISC_CPRICE_M',
            style: 'text-align:left',
            width: 130,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "訂單優惠最小單價",
            dataIndex: 'DISC_UPRICE',
            style: 'text-align:left',
            width: 130,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行優惠最小單價",
            dataIndex: 'DISC_UPRICE_M',
            style: 'text-align:left',
            width: 130,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "本次進貨量",
            dataIndex: 'INQTY',
            style: 'text-align:left',
            width: 110,
            align: 'right'
        }, {
            text: "小計",
            dataIndex: 'ACC_AMT',
            style: 'text-align:left',
            width: 90,
            align: 'right'
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            style: 'text-align:left',
            width: 90,
            align: 'left'
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            style: 'text-align:left',
            width: 100,
            align: 'left'
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            style: 'text-align:left',
            width: 80,
            align: 'left'
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            style: 'text-align:left',
            width: 100,
            align: 'left'
        }, {
            text: "折讓金額",
            dataIndex: 'EXTRA_DISC_AMOUNT',
            style: 'text-align:left',
            width: 130,
            align: 'right'
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            width: 150,
            align: 'left'
        }, {
            text: "病人姓名",
            dataIndex: 'CHINNAME',
            style: 'text-align:left',
            width: 90,
            align: 'left'
        }, {
            text: "病歷號",
            dataIndex: 'CHARTNO',
            style: 'text-align:left',
            width: 120,
            align: 'left'
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
                setFormT11a();
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query],
            resizable:true
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 140,
            sortable: true
        }, {
            text: "訂單日期",
            dataIndex: 'PO_TIME',
            width: 80,
            sortable: true
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO',
            width: 80,
            sortable: true
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 250,
            sortable: true
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                T21Load();
            }
        }
    });

    var T21Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T21Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T21Query],
	        resizable: true
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T21Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 100,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 100,
            sortable: true
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 100,
            sortable: true
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 80,
            sortable: true
        }, {
            text: "借貨量",
            dataIndex: 'BW_SQTY',
            style: 'text-align:left',
            width: 70,
            align: 'right'
        }, {
            text: "進貨接收量",
            dataIndex: 'ACC_QTY',
            style: 'text-align:left',
            width: 110,
            align: 'right'
        }, {
            text: "訂單單價", // 訂單單價
            dataIndex: 'ACC_PO_PRICE',
            style: 'text-align:left',
            width: 90,
            align: 'right'
        }, {
            text: "訂單單價小計", // 訂單單價
            dataIndex: 'PO_PRICE_AMT',
            style: 'text-align:left',
            width: 110,
            align: 'right'
        }, {
            text: "優惠單價", // 訂單優惠合約單價
            dataIndex: 'ACC_DISC_CPRICE',
            style: 'text-align:left',
            width: 90,
            align: 'right'
        }, {
            text: "<b><font color=red>小計</font></b>",
            dataIndex: 'ACC_AMT',
            style: 'text-align:left',
            width: 90,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/ // 用正規表示式限制可輸入內容
            },
            align: 'right'
        }, {
            text: "最小計量單位",
            dataIndex: 'ACC_BASEUNIT',
            width: 100,
            sortable: true
        }, {
            xtype: 'datecolumn',
            text: "<b><font color=red>發票日期</font></b>",
            dataIndex: 'INVOICE_DT',
            format: 'Xmd',
            width: 100,
            editor: {
                xtype: 'datefield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                format: 'Xmd',
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'Xmd');
                }
            }, align: 'right'
        }, {
            text: "<b><font color=red>發票號碼</font></b>",
            dataIndex: 'INVOICE',
            width: 100,
            editor: {
                xtype: 'textfield',
                maxLength: 10,
                regex: /^[A-Z]{2}[0-9]{8}$/,
            }, align: 'right'
        }, {
            text: "<b><font color=red>備註</font></b>",
            dataIndex: 'MEMO',
            width: 100,
            editor: {
                xtype: 'textfield',
                maxLength: 150
            }
        }, {
            text: "驗收確認量",
            dataIndex: 'CFM_QTY',
            style: 'text-align:left',
            width: 110,
            align: 'right'
        }, {
            text: "狀態",
            dataIndex: 'STATUS',
            width: 70,
            sortable: true
        }, {
            text: "進貨日期",
            dataIndex: 'ACC_TIME',
            width: 80,
            sortable: true
        }, {
            text: "進貨人員",
            dataIndex: 'ACC_USER',
            width: 100,
            sortable: true
        }, {
            text: "庫備標示",
            dataIndex: 'STOREID',
            width: 100,
            sortable: true
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS',
            width: 100,
            sortable: true
        }, {
            text: "訂單數量",
            dataIndex: 'PO_QTY',
            style: 'text-align:left',
            width: 100,
            align: 'right'
        }, {
            text: "申購計量單位",
            dataIndex: 'ACC_PURUN',
            width: 100,
            sortable: true
        }, {
            text: "轉換率",
            dataIndex: 'UNIT_SWAP',
            width: 60,
            sortable: true
        }, {
            text: "批號效期註記",
            dataIndex: 'WEXP_ID',
            width: 100,
            sortable: true
        }, {
            text: "入帳數量",
            dataIndex: 'TX_QTY_T',
            style: 'text-align:left',
            width: 80,
            align: 'right'
        }, {
            text: "折讓金額",
            dataIndex: 'EXTRA_DISC_AMOUNT',
            style: 'text-align:left',
            width: 110,
            align: 'right'
        }, {
            text: "病人姓名",
            dataIndex: 'CHINNAME',
            width: 80,
            sortable: true
        }, {
            text: "病歷號",
            dataIndex: 'CHARTNO',
            width: 110,
            sortable: true
        }, {
            text: "序號",
            dataIndex: 'SEQ',
            width: 110,
            renderer: function (val, meta, record) {
                if (record.data['REF_CNT'] > 0)
                    return "<font color='blue'>" + val + "</font>";
                else
                    return val;
            }
        }, {
            text: "還原來源序號",
            dataIndex: 'SRC_SEQ',
            width: 120
        }, {
            header: "",
            flex: 1
        }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                        if (T21LastRec) {
                            // [還原為未進貨]和有做過[還原為未進貨]的資料不允許編輯
                            if (T21LastRec.data.TX_QTY_T > 0 && T21LastRec.data.REF_CNT == 0)
                                return true;
                            else
                                return false;
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                    }
                }
            })
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            },
            selectionchange: function (model, records) {
                T21Rec = records.length;
                T21LastRec = records[0];
                if (T21LastRec) {
                    // [還原為未進貨]和有做過[還原為未進貨]的資料不可使用[還原為未進貨]
                    if (T21LastRec.data.TX_QTY_T > 0 && T21LastRec.data.REF_CNT == 0)
                        T21Grid.down('#trans_back').setDisabled(false);
                    else
                        T21Grid.down('#trans_back').setDisabled(true);
                }
            }
        }
    });

    // 設定欄位是否顯示
    var setVisibleColumns = function (optVal) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        T11Grid.suspendLayouts();
        for (var i = 1; i < T11Grid.columns.length; i++) {
            if (T11Grid.columns[i].showOption == optVal)
                T11Grid.columns[i].setVisible(false);
            else
                T11Grid.columns[i].setVisible(true);
        }
        T11Grid.resumeLayouts(true);
        myMask.hide();
    }

    // 變更單價
    chgPrice = function (parPO_NO, parMMCODE, parPO_PRICE, parM_CONTPRICE) {
        Ext.MessageBox.confirm('變更單價', '是否確定更改' + parPO_NO + ' ' + parMMCODE + '訂單單價，從' + parPO_PRICE + '改為' + parM_CONTPRICE + '?', function (btn, text) {
            if (btn === 'yes') {
                Ext.Ajax.request({
                    url: priceSet,
                    params: {
                        po_no: parPO_NO,
                        mmcode: parMMCODE
                    },
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            T11Load();
                        }
                    }
                });
            }
        }
        );
    }

    function chkT1QueryValid() {
        if (T1Query.getForm().isValid()) {
            return true;
        } else {
            Ext.Msg.alert('提醒', '查詢條件不合規則');
            return false;
        }
    }

    function chkT2QueryValid() {
        if (T2Query.getForm().isValid()) {
            return true;
        } else {
            Ext.Msg.alert('提醒', '查詢條件不合規則');
            return false;
        }
    }

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 5
        },
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            width: 200,
            labelAlign: "right"
        },
        items: [{
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '',
            name: 'MMNAME_C',
            width: 800,
            colspan: 4
        }, {
            xtype: 'displayfield',
            fieldLabel: '總訂購量',
            name: 'PO_QTY'
        }, {
            fieldLabel: '本次進貨量',
            name: 'INQTY',
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            readOnly: true,
            fieldCls: 'required',
            allowBlank: false,
            listeners: {
                blur: function (self, record, event, eOpts) {
                    calAmt(true);
                },
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('LOT_NO').focus();
                    }
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '申購計量單位',
            name: 'M_PURUN',
            colspan: 3
        }, {
            fieldLabel: '批號',
            name: 'LOT_NO',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            listeners: {
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('EXP_DATE').focus();
                    }
                }
            }
        }, {
            xtype: 'datefield',
            fieldLabel: '效期',
            name: 'EXP_DATE',
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            format: 'Xmd',
            enforceMaxLength: true,
            maxLength: 7,
            readOnly: true,
            renderer: function (value, meta, record) {
                return Ext.util.Format.date(value, 'Xmd');
            },
            listeners: {
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('INVOICE_DT').focus();
                    }
                }
            }
        }, {
            xtype: 'datefield',
            fieldLabel: '發票日期',
            name: 'INVOICE_DT',
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            format: 'Xmd',
            enforceMaxLength: true,
            maxLength: 7,
            readOnly: true,
            renderer: function (value, meta, record) {
                return Ext.util.Format.date(value, 'Xmd');
            },
            listeners: {
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('INVOICE').focus();
                    }
                }
            }
        }, {
            fieldLabel: '發票號碼',
            name: 'INVOICE',
            regexText: '需輸入10碼發票號碼',
            // regex: /^[A-Za-z]{2}[0-9]{8}$/, // 開頭兩碼英文(發票號碼)
            regex: /^[A-Za-z0-9]{10}$/, // 新竹說可能會填收據,改為只限制10碼
            enforceMaxLength: true,
            maxLength: 10,
            readOnly: true,
            listeners: {
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('STORE_LOC').focus();
                    }
                }
            }
        }, {
            fieldLabel: '儲位',
            name: 'STORE_LOC',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            listeners: {
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('EXTRA_DISC_AMOUNT').focus();
                    }
                }
            }
        }, {
            xtype: 'numberfield',
            fieldLabel: '折讓金額',
            name: 'EXTRA_DISC_AMOUNT',
            regexText: '只能輸入數字,至多小數2位',
            regex: /^[0-9]+(\.[0-9]{1,2})?$/,
            readOnly: true,
            listeners: {
                change: function (_this, newvalue, oldvalue) {
                    calAmt(true);
                },
                specialkey: function (f, e) {
                    if (e.getKey() == e.ENTER) {
                        T1Form.getForm().findField('ACC_AMT').focus();
                    }
                }
            }
            /*   }, { //北投1121018反映進貨單價不能修改
                   xtype: 'numberfield',
                   fieldLabel: '訂單單價', // 訂單單價
                   name: 'PO_PRICE',
                   regexText: '只能輸入數字,至多小數2位',
                   regex: /^[0-9]+(\.[0-9]{1,2})?$/,
                   readOnly: true,
                   fieldCls: 'required',
                   allowBlank: false,
                   listeners: {
                       change: function (_this, newvalue, oldvalue) {
                           calAmt(true);
                       }
                   }*/
        }, {
            xtype: 'displayfield',
            fieldLabel: '訂單單價',
            name: 'PO_PRICE',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '訂單單價小計',
            name: 'PO_PRICE_AMT'
            /*20231211 優惠單價不可修改
        }, {
            xtype: 'numberfield',
            fieldLabel: '優惠單價', // 訂單優惠合約單價
            name: 'DISC_CPRICE',
            regexText: '只能輸入數字,至多小數3位',
            regex: /^[0-9]+(\.[0-9]{1,3})?$/,
            decimalPrecision: 3,
            readOnly: true,
            fieldCls: 'required',
            allowBlank: false,
            listeners: {
                change: function (_this, newvalue, oldvalue) {
                    calAmt(true);
                }
            }
            */
        }, {
            xtype: 'displayfield',
            fieldLabel: '優惠單價',
            name: 'DISC_CPRICE',
            submitValue: true
        }, {
            xtype: 'numberfield',
            fieldLabel: '小計',
            name: 'ACC_AMT',
            //regexText: '只能輸入數字,至多小數2位',
            //regex: /^[0-9]+(\.[0-9]{1,2})?$/,
            regexText: '只能輸入數字',
            regex: /^[0-9]+$/,
            decimalPrecision: 0,
            readOnly: true,
            fieldCls: 'required',
            allowBlank: false,
            listeners: {
                change: function (_this, newvalue, oldvalue) {
                    calAmt(false);
                }
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '合約成本差額',
            name: 'ACC_AMT_DIS'
        }, {
            fieldLabel: '備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 300,
            width: 800,
            readOnly: true,
            colspan: 4
        }, {
            name: 'PO_NO',
            xtype: 'hidden'
        }, {
            name: 'POACC_SEQ',
            xtype: 'hidden'
        }, {
            name: 'EXP_DATE_RAW',
            xtype: 'hidden'
        }, {
            name: 'INVOICE_DT_RAW',
            xtype: 'hidden'
        }, {
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    Ext.MessageBox.confirm('儲存', '是否確定儲存?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.findField('EXP_DATE_RAW').setValue(f.findField('EXP_DATE').rawValue);
            f.findField('INVOICE_DT_RAW').setValue(f.findField('INVOICE_DT').rawValue);
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var msg = action.result.msg;

                    switch (f2.findField("x").getValue()) {
                        case "U":
                            msglabel('訊息區:資料更新成功');
                            T11Load();
                            break;
                    }

                    T11Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function T11Cleanup() {
        viewport.down('#t11Grid').unmask();
        var f = T1Form.getForm();
        f.reset();

        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            f.setReadOnly(true);
        });

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('明細');

        setFormT11a();
    }

    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "未進貨":
                        // 點未進貨標籤時先重新載入以取得最新資料,以免若有還原為未進貨,資料不是最新的
                        T11Cleanup();
                        T11Load();
                        break;
                    case "已進貨":
                        break;
                }
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            title: '未進貨',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                region: 'west',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                width: 240,
                items: [T1Grid]
            }, {
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
                    // width: '80%',
                    flex: 1,
                    minWidth: 50,
                    minHeight: 140,
                    items: [
                        {
                            itemId: 't11Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            split: true,
                            // width: '80%',
                            items: [T11Grid]
                        },
                        {
                            itemId: 'form',
                            region: 'south',
                            collapsible: true,
                            floatable: true,
                            split: true,
                            minWidth: 120,
                            minHeight: 140,
                            height: 220,
                            title: '明細',
                            collapsed: true,
                            border: false,
                            layout: {
                                type: 'fit',
                                padding: 5,
                                align: 'stretch'
                            },
                            items: [T1Form]
                        }
                    ]
                }]
            }
            ]
        },
        {
            title: '已進貨',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 'T21Grid',
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
                    // width: '80%',
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
                            height: '40%',
                            items: [T2Grid]
                        }, {
                            region: 'north',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            split: true,
                            height: '60%',
                            items: [T21Grid]
                        }
                    ]
                }]
            }
            ]
        }
        ]
    });

    var SplitFormWin = null;
    popSplitFormWin = function (pMmcode, pPoqty, pDeliqty) {
        var SplitFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelWidth: 130,
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelSplitForm',
                border: false,
                layout: 'vbox',
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        padding: '0 4 0 4',
                        width: '100%',
                        readOnly: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '進貨量',
                        name: 'INQTY',
                        padding: '0 4 0 4',
                        width: '100%',
                        readOnly: true
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '拆項數',
                        name: 'SPLIT_QTY',
                        maxValue: pPoqty,
                        minValue: 2,
                        allowDecimals: false, //是否允許輸入小數
                        keyNavEnabled: false,
                        mouseWheelEnabled: false,
                        fieldCls: 'required',
                        allowBlank: false,
                        padding: '0 4 0 4',
                        width: '100%',
                        value: 2,
                        listeners: {
                            blur: function (filed, event, eOpts) {
                                if (SplitFormContent.getForm().findField('MMCODE').getValue()) {
                                    if (!SplitFormContent.getForm().findField('SPLIT_QTY').getValue())
                                        SplitFormContent.getForm().findField('SPLIT_QTY').setValue('2');
                                    var nQty = pPoqty - pDeliqty;
                                    var sQty = SplitFormContent.getForm().findField('SPLIT_QTY').getValue();
                                    var dQty = Math.floor(nQty / sQty);
                                    var rQty = nQty % sQty;
                                    SplitFormContent.getForm().findField('INQTY').setValue(nQty + ' = ' + (rQty + dQty) + '*(1項) + ' + dQty + '*(' + (sQty - 1) + '項)');
                                }
                            }
                        }
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '備註',
                        name: 'MEMO',
                        padding: '0 4 0 4',
                        width: '100%'
                    }
                ]
            }]
        });

        var SplitForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [SplitFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '拆項',
                handler: function () {
                    var nQty = pPoqty - pDeliqty;
                    if (!SplitFormContent.getForm().findField('SPLIT_QTY').getValue()) {
                        Ext.Msg.alert('訊息', '拆項數為必填且需大於0');
                        return;
                    }
                    else if (SplitFormContent.getForm().findField('SPLIT_QTY').getValue() > nQty) {
                        Ext.Msg.alert('訊息', '拆項數不可大於剩餘訂購量');
                        return;
                    }
                    Ext.MessageBox.confirm('訊息', '是否確定將院內碼' + SplitFormContent.getForm().findField('MMCODE').getValue() + '拆分為' + SplitFormContent.getForm().findField('SPLIT_QTY').getValue() + '項?', function (btn, text) {
                        if (btn === 'yes') {
                            var P0 = T11LastRec.data.PO_NO;
                            var P1 = T11LastRec.data.POACC_SEQ;
                            var P2 = SplitFormContent.getForm().findField('SPLIT_QTY').getValue();
                            var P3 = SplitFormContent.getForm().findField('MEMO').getValue();
                            Ext.Ajax.request({
                                url: splitMmcode,
                                method: reqVal_p,
                                params: {
                                    p0: P0,
                                    p1: P1,
                                    p2: P2,
                                    p3: P3
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        SplitFormWin.destroy();
                                        SplitFormWin = null;
                                        T11Load();
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {

                                }
                            });
                        }
                    }
                    );
                }
            }, {
                disabled: false,
                text: '關閉',
                handler: function () {
                    SplitFormWin.destroy();
                    SplitFormWin = null;
                    T11Load();
                }
            }]
        });
        var formWinTitle = pMmcode + '總訂購量拆項';
        SplitFormWin = GetPopWin(viewport, SplitForm, formWinTitle, 370, 250);

        SplitFormWin.show();

        var nQty = pPoqty - pDeliqty;
        SplitFormContent.getForm().findField('MMCODE').setValue(pMmcode);
        SplitFormContent.getForm().findField('INQTY').setValue(nQty);
    }

    var InvoiceFormWin = null;
    popInvoiceFormWin = function () {
        var InvoiceFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelWidth: 130,
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelInvoiceForm',
                border: false,
                layout: 'vbox',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '批號',
                    name: 'LOT_NO',
                    enforceMaxLength: true,
                    maxLength: 20,
                    width: '100%',
                    listeners: {
                        specialkey: function (f, e) {
                            if (e.getKey() == e.ENTER) {
                                InvoiceFormContent.getForm().findField('EXP_DATE').focus();
                            }
                        }
                    }
                }, {
                    xtype: 'datefield',
                    fieldLabel: '效期',
                    name: 'EXP_DATE',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    format: 'Xmd',
                    enforceMaxLength: true,
                    maxLength: 7,
                    width: '100%',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    },
                    listeners: {
                        specialkey: function (f, e) {
                            if (e.getKey() == e.ENTER) {
                                InvoiceFormContent.getForm().findField('INVOICE_DT').focus();
                            }
                        }
                    }
                }, {
                    xtype: 'datefield',
                    fieldLabel: '發票日期',
                    name: 'INVOICE_DT',
			        id:'popFormINVOICE_DT',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    format: 'Xmd',
                    allowBlank: true,
                    enforceMaxLength: true,
                    maxLength: 7,
                    width: '100%',
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'Xmd');
                    },
                    listeners: {
                         specialkey: function (f, e) {
                            if (e.getKey() == e.ENTER) {
                              InvoiceFormContent.getForm().findField('INVOICE').focus();
                            }
                         }
                     }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '發票號碼',
                    name: 'INVOICE',
                    allowBlank: true,
                    enforceMaxLength: true,
                    maxLength: 10,
                    width: '100%',
                    regexText: '需輸入10碼發票號碼',
                    // regex: /^[A-Za-z]{2}[0-9]{8}$/, // 開頭兩碼英文(發票號碼)
                    regex: /^[A-Za-z0-9]{10}$/, // 新竹說可能會填收據,改為只限制10碼
                    enforceMaxLength: true,
                    maxLength: 10,
                    listeners: {
                         specialkey: function (f, e) {
                            if (e.getKey() == e.ENTER) {
                                //InvoiceFormContent.getForm().findField('INVOICE').focus();
                                InvoiceFormContent.getForm().findField('EXTRA_DISC_AMOUNT').focus();
                            }
                         }
                     }
                }, {
                    xtype: 'numberfield',
                    fieldLabel: '折讓金額',
                    name: 'EXTRA_DISC_AMOUNT',
                    regexText: '只能輸入數字,至多小數2位',
                    regex: /^[0-9]+(\.[0-9]{1,2})?$/,
                    width: '100%',
                    listeners: {
                        specialkey: function (f, e) {
                            if (e.getKey() == e.ENTER) {
                                InvoiceFormContent.getForm().findField('MEMO').focus();
                            }
                        }
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '備註',
                    name: 'MEMO',
                    enforceMaxLength: true,
                    maxLength: 300,
                    width: '100%',
                    listeners: {
                        specialkey: function (f, e) {
                            if (e.getKey() == e.ENTER) {
                                Ext.getCmp('enterInvoiceDone').el.dom.click();
                            }
                        }
                    }
                }]
            }]
        });

        var InvoiceForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [InvoiceFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '批次填入',
	        id:'enterInvoiceDone',
                handler: function () {
                    if (InvoiceFormContent.getForm().isValid()) {
                        Ext.MessageBox.confirm('訊息', '是否確定將填寫資料填入勾選的項目?', function (btn, text) {
                            if (btn === 'yes') {
                                var selection = T11Grid.getSelection();
                                var poacc_seq = '';
                                if (selection.length) {
                                    $.map(selection, function (item, key) {
                                        poacc_seq += item.get('POACC_SEQ') + '^';
                                    })
                                }

                                var P0 = T11LastRec.data.PO_NO;
                                var P1 = poacc_seq;
                                var P2 = InvoiceFormContent.getForm().findField('LOT_NO').getValue();
                                var P3 = InvoiceFormContent.getForm().findField('EXP_DATE').rawValue;
                                var P4 = InvoiceFormContent.getForm().findField('INVOICE').getValue();
                                var P5 = InvoiceFormContent.getForm().findField('INVOICE_DT').rawValue;
                                var P6 = InvoiceFormContent.getForm().findField('EXTRA_DISC_AMOUNT').getValue();
                                var P7 = InvoiceFormContent.getForm().findField('MEMO').getValue();
                                Ext.Ajax.request({
                                    url: impInvoice,
                                    method: reqVal_p,
                                    params: {
                                        p0: P0,
                                        p1: P1,
                                        p2: P2,
                                        p3: P3,
                                        p4: P4,
                                        p5: P5,
                                        p6: P6,
                                        p7: P7
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            InvoiceFormWin.destroy();
                                            InvoiceFormWin = null;
                                            T11Load();
                                        } else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                        }
                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }
                        }
                        );
                    }
                }
            }, {
                disabled: false,
                text: '關閉',
                handler: function () {
                    InvoiceFormWin.destroy();
                    InvoiceFormWin = null;
                    T11Load();
                }
            }]
        });
        InvoiceFormWin = GetPopWin(viewport, InvoiceForm, '批次填入', 330, 260);

        InvoiceFormWin.show();
        Ext.getCmp('popFormINVOICE_DT').focus();
    }

    // 匯入
    var ImportStore = Ext.create('Ext.data.Store', {
        model: 'T11Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
            }
        }
    })

    var ImportQuery = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 't11export',
                name: 't11export',
                text: '範本', handler: function () {
                    var p = new Array();
                    PostForm(ImportGetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'ImportSend',
                id: 'ImportSend',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        Ext.getCmp('T11insert').setDisabled(true);
                        ImportStore.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('ImportSend').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0176/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        ImportStore.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T11insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        ImportStore.loadData(data.etts, false);
                                        IsSend = true;
                                        ImportGrid.columns[1].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('T11insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('ImportSend').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('ImportSend').fileInputEl.dom.value = '';
                                    Ext.getCmp('T11insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '更新',
                id: 'T11insert',
                name: 'T11insert',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0176/Insert',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(ImportStore.data.items, 'data'))
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");

                                Ext.getCmp('T11insert').setDisabled(true);
                                ImportStore.removeAll();
                                ImportGrid.columns[1].setVisible(false);
                                T11Load();
                            }
                            myMask.hide();
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWinImport();
                }
            }
        ]
    });

    function ImportCleanup() {
        ImportQuery.getForm().reset();
        msglabel('訊息區:');
    }
    var ImportGrid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: ImportStore,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [ImportQuery]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        //selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            hidden: true,
            width: 250
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 120,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
            sortable: true
        }, {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            width: 100
        }, {
            text: "本次進貨量",
            dataIndex: 'INQTY',
            style: 'text-align:left',
            width: 110, align: 'right'
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 100
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 100
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            width: 100
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            width: 120
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 100
        }, {
            header: "",
            flex: 1
        }
        ]
    });

    function T11ChkBtns() {
        var selection = T11Grid.getSelection();
        if (selection.length) {
            if (selection.length == 1) {
                // 依花蓮庫房需求, 選擇一筆資料時, 自動進入修改模式
                //T11Grid.down('#edit').setDisabled(false);
                setFormT11("U", '修改');
                T11Grid.down('#add').setDisabled(false);
                T11Grid.down('#splitBtn').setDisabled(false);
                T11Grid.down('#delBtn').setDisabled(false);
                T11Grid.down('#importInvoice').setDisabled(false);
                T11Grid.down('#completeBtn').setDisabled(false);
            }
            else {
                //T11Grid.down('#edit').setDisabled(true);
                T11Cleanup();
                T11Grid.down('#add').setDisabled(false);
                T11Grid.down('#splitBtn').setDisabled(true);
                T11Grid.down('#delBtn').setDisabled(true);
                T11Grid.down('#importInvoice').setDisabled(false);
                T11Grid.down('#completeBtn').setDisabled(true);
            }
        }
        else {
            //T11Grid.down('#edit').setDisabled(true);
            T11Cleanup();
            T11Grid.down('#add').setDisabled(true);
            T11Grid.down('#splitBtn').setDisabled(true);
            T11Grid.down('#delBtn').setDisabled(true);
            T11Grid.down('#importInvoice').setDisabled(true);
            T11Grid.down('#completeBtn').setDisabled(true);
        }
    }

    // 更新訂單單價小計.小計.合約成本差額
    function calAmt(calAll) {
        var f = T1Form.getForm();

        var pINQTY = f.findField('INQTY').getValue();
        if (isNaN(Number(pINQTY)) || pINQTY == '')
            pINQTY = 0;
        var pEXTRA_DISC_AMOUNT = f.findField('EXTRA_DISC_AMOUNT').getValue();
        if (isNaN(Number(pEXTRA_DISC_AMOUNT)) || pEXTRA_DISC_AMOUNT == '')
            pEXTRA_DISC_AMOUNT = 0;
        var pPO_PRICE = f.findField('PO_PRICE').getValue();
        if (isNaN(Number(pPO_PRICE)) || pPO_PRICE == '')
            pPO_PRICE = 0;
        var pDISC_CPRICE = f.findField('DISC_CPRICE').getValue();
        if (isNaN(Number(pDISC_CPRICE)) || pDISC_CPRICE == '')
            pDISC_CPRICE = 0;

        var pPOAMT = Math.round(pINQTY * pPO_PRICE - pEXTRA_DISC_AMOUNT); // 訂單單價小計 = 本次進貨量 * 訂單單價 - 折讓金額
        var pACCAMT = f.findField('ACC_AMT').getValue(); // 若calAll為false則使用ACC_AMT填寫的值(可能是已儲存的值或使用者修改的值)
        if (calAll)
            pACCAMT = Math.round(pINQTY * pDISC_CPRICE - pEXTRA_DISC_AMOUNT); // 小計 = 本次進貨量 * 優惠單價 - 折讓金額
        f.findField('PO_PRICE_AMT').setValue(pPOAMT);
        f.findField('ACC_AMT').setValue(pACCAMT);
        f.findField('ACC_AMT_DIS').setValue(Math.round(pPOAMT - pACCAMT));
    }

    function calAmtMsg() {
        // 統計未進貨-畫面上方各項合計
        Ext.Ajax.request({
            url: amtGet,
            method: reqVal_p,
            params: {
                PO_NO: T1LastRec.data.PO_NO
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '') {
                        T11Query.down('#T11Query_AmtMsg').setText(data.msg);
                    }
                } else {
                    T11Query.down('#T11Query_AmtMsg').setText('');
                }
            },
            failure: function (response, options) {
            }
        });

    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: TATabs
    });

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;
    var winImport;
    if (!winImport) {
        winImport = Ext.widget('window', {
            title: '匯入',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [ImportGrid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWinImport() {
        if (winImport.hidden) {
            ImportCleanup();
            ImportStore.removeAll();
            winImport.show();
        }
    }
    function hideWinImport() {
        if (!winImport.hidden) {
            winImport.hide();
            ImportCleanup();
        }
    }

    T1Query.getForm().findField('P0').focus();
});
