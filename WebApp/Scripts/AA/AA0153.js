Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Name = "進貨接收";
    var qtySet = '/api/AA0153/SetQty';
    var priceSet = '/api/AA0153/SetPrice';
    var transSet = '/api/AA0153/SetTrans';
    var transBackSet = '/api/AA0153/SetTransBack';

    var T1Rec = 0;
    var T1LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;

    var T11Rec = 0;
    var T11LastRec = null;
    var T21Rec = 0;
    var T21LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0153/GetMatClassCombo',
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
        queryUrl: '/api/AA0153/GetAgennoCombo',
        width: 150,
        matchFieldWidth: false,
        listConfig: { width: 300 }
    });
    var T21QueryAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'T21P5',
        id: 'T21P5',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: '/api/AA0153/GetAgennoCombo',
        width: 150,
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
                id: 'PanelT1P1',
                border: false,
                layout: 'hbox',
                autoScroll: true,
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料類別',
                        name: 'P0',
                        id: 'P0',
                        width: 200,
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
                        name: 'P1',
                        id: 'P1',
                        width: 200
                    }, {
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
                        value: new Date()
                    }, T11QueryAgenno,
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'P6',
                        id: 'P6',
                        width: 180
                    },
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
                id: 'PanelT11P1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'P2',
                        id: 'P2',
                        width: 200
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
        bodyPadding: '0 5 0 5',
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
                        width: 200
                    },{
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
        height: 75,
        bodyPadding: '5 0 5 0',
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
                padding: '10 5 5 10',
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
            { name: 'PO_TIME', type: 'string' }
        ]
    });
    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'MAT_CLSNAME', type: 'string' },
            { name: 'PO_TIME', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'STORE_LOC', type: 'string' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'DELI_QTY', type: 'string' },
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
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'EXTRA_DISC_AMOUNT', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'DELI_AMT', type: 'string' },
            { name: 'IN_CONT_AMT', type: 'string' },
            { name: 'IN_PO_AMT', type: 'string' },
            { name: 'IN_DISC_AMT', type: 'string' }
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
            { name: 'SRC_SEQ', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'REF_CNT', type: 'string' },
            { name: 'STATUS_N', type: 'string'}
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'PO_NO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0153/AllM',
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
                        p6: T1Query.getForm().findField('P6').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else
                    return false;
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
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0153/AllD',
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
            }
        }
    });
    function T11Load() {
        T11Store.load({
            params: {
                start: 0
            }
        });
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
            {
                itemId: 'add', text: '進貨', handler: function () {
                    var store = T11Grid.getStore().data.items;

                    var chkResult = '';
                    var chkResultMsg1 = '';
                    for (var i = 0; i < store.length; i++) {
                        if (store[i].data.INQTY > 0) {
                            // 本次進貨量 編輯欄位是否有填寫
                            if (store[i].data.INQTY == '') {
                                Ext.Msg.alert('錯誤', '院內碼 ' + store[i].data.MMCODE + '進貨量未填寫完成，請重新確認');
                                return;
                            }
                            // 折讓金額 不可大於優惠價小計
                            else if (store[i].data.EXTRA_DISC_AMOUNT > store[i].data.IN_DISC_AMT) {
                                Ext.Msg.alert('錯誤', '院內碼 ' + store[i].data.MMCODE + '折讓金額不可大於優惠價小計，請重新確認');
                                return;
                            }
                            // 總進貨量是否大於訂貨量(只提示，若不通過仍可確認進貨)
                            else if (Number(store[i].data.PO_QTY) < Number(store[i].data.DELI_QTY) + Number(store[i].data.INQTY)) {
                                chkResult = '1';
                                chkResultMsg1 += store[i].data.MMCODE + '<br>';
                            }
                        }
                    }
                    var confirmMsg = '是否確定進貨?';
                    if (chkResult == '1')
                        confirmMsg = '院內碼<br>' + chkResultMsg1 + ' 總進貨量大於訂貨量，是否確定進貨?';
                    Ext.MessageBox.confirm('進貨', confirmMsg, function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var list = [];
                            for (var i = 0; i < store.length; i++) {
                                if (store[i].data.INQTY > 0) {
                                    // 折讓金額未填則代0
                                    var vEXTRA_DISC_AMOUNT = 0;
                                    if (store[i].data.EXTRA_DISC_AMOUNT > 0)
                                        vEXTRA_DISC_AMOUNT = store[i].data.EXTRA_DISC_AMOUNT;
                                    // 儲位未填則代預設值
                                    var vSTORE_LOC = 'TMPLOC';
                                    if (store[i].data.STORE_LOC != '')
                                        vSTORE_LOC = store[i].data.STORE_LOC;
                                    // 批號效期未填則代預設值
                                    var vLOT_NO = 'TMPLOT';
                                    if (store[i].data.LOT_NO != '')
                                        vLOT_NO = store[i].data.LOT_NO;
                                    var vEXP_DATE = '9991231';
                                    if (store[i].data.EXP_DATE != '')
                                        vEXP_DATE = Ext.util.Format.date(store[i].data.EXP_DATE, 'Xmd');

                                    var item = {
                                        PO_NO: store[i].data.PO_NO,
                                        MMCODE: store[i].data.MMCODE,
                                        STORE_LOC: vSTORE_LOC,
                                        INQTY: store[i].data.INQTY,
                                        ACCQTY: store[i].data.ACCQTY,
                                        LOT_NO: vLOT_NO,
                                        EXP_DATE: vEXP_DATE,
                                        INVOICE: store[i].data.INVOICE,
                                        INVOICE_DT: Ext.util.Format.date(store[i].data.INVOICE_DT, 'Xmd'),
                                        EXTRA_DISC_AMOUNT: vEXTRA_DISC_AMOUNT,
                                        MEMO: store[i].data.MEMO,
                                        MAT_CLASS: store[i].data.MAT_CLASS,
                                        CHINNAME: store[i].data.CHINNAME,
                                        CHARTNO: store[i].data.CHARTNO,
                                        PO_D_SEQ: store[i].data.SEQ
                                    }
                                    list.push(item);
                                }
                            }
                            if (list.length == 0) {
                                Ext.Msg.alert('訊息', '尚未填寫任何進貨資料，請確認!');
                                myMask.hide();
                                return;
                            }
                            Ext.Ajax.request({
                                url: qtySet,
                                method: reqVal_p,
                                params: {
                                    list: Ext.util.JSON.encode(list)
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
            url: '/api/AA0153/AllAccLogM',
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
        T2Store.load({
            params: {
                start: 0
            }
        });
    }

    var T21Store = Ext.create('Ext.data.Store', {
        model: 'T21Model',
        pageSize: 10, // 每頁顯示筆數
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
            url: '/api/AA0153/AllAccLogD',
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
            items: [T1Query]
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
                T1Rec = records.length;
                T1LastRec = records[0];
                T11Load();
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
            items: [T11Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }
        ],
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
            text: "申購計量單位",
            dataIndex: 'M_PURUN',
            width: 100,
            sortable: true
        }, {
            text: "<b><font color=red>儲位</font></b>",
            dataIndex: 'STORE_LOC',
            style: 'text-align:left',
            width: 100,
            editor: {
                xtype: 'textfield',
                maxLength: 20,
                emptyText: 'TMPLOC'
            }, align: 'left'
        }, {
            text: "<b><font color=red>本次進貨量</font></b>",
            dataIndex: 'INQTY',
            style: 'text-align:left',
            width: 100,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                listeners: {
                    blur: function (self, record, event, eOpts) {

                        var index = -1;
                        var store = T11Grid.getStore().data.items;
                        for (var i = 0; i < store.length; i++) {
                            if (store[i] == T11LastRec) {
                                index = i;
                            }
                        }

                        if (isNaN(Number(self.value)) || self.value == '') {
                            self.setValue('');
                            T11LastRec.set('INQTY', '');
                            T11LastRec.set('ACC_QTY', '');
                            T11LastRec.set('IN_CONT_AMT', '');
                            T11LastRec.set('IN_PO_AMT', '');
                            T11LastRec.set('IN_DISC_AMT', '');
                            T11Grid.getStore().data.items[index].data.INQTY = '';
                            T11Grid.getStore().data.items[index].data.ACC_QTY = '';
                            T11Grid.getStore().data.items[index].data.IN_CONT_AMT = '';
                            T11Grid.getStore().data.items[index].data.IN_PO_AMT = '';
                            T11Grid.getStore().data.items[index].data.IN_DISC_AMT = '';
                            return;
                        }
                        var acc_qty = self.value * T11LastRec.data.UNIT_SWAP;
                        var in_cont_amt = Math.round(self.value * T11LastRec.data.M_CONTPRICE); //現行單價小計
                        var in_po_amt = Math.round(self.value * T11LastRec.data.PO_PRICE); //合約價小計
                        var in_disc_amt = Math.round(self.value * T11LastRec.data.DISC_CPRICE); //優惠價小計
                        T11LastRec.set('INQTY', self.value);
                        T11LastRec.set('ACC_QTY', acc_qty);
                        T11LastRec.set('IN_CONT_AMT', in_cont_amt);
                        T11LastRec.set('IN_PO_AMT', in_po_amt);
                        T11LastRec.set('IN_DISC_AMT', in_disc_amt);

                        T11Grid.getStore().data.items[index].data.INQTY = self.value;
                        T11Grid.getStore().data.items[index].data.ACC_QTY = acc_qty;
                        T11Grid.getStore().data.items[index].data.IN_CONT_AMT = in_cont_amt;
                        T11Grid.getStore().data.items[index].data.IN_PO_AMT = in_po_amt;
                        T11Grid.getStore().data.items[index].data.IN_DISC_AMT = in_disc_amt;
                    }
                }
            }, align: 'right'
        }, {
            xtype: 'datecolumn',
            text: "<b><font color=red>發票日期</font></b>",
            dataIndex: 'INVOICE_DT',
            style: 'text-align:left',
            width: 90,
            format: 'Xmd',
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
            style: 'text-align:left',
            width: 100,
            editor: {
                xtype: 'textfield',
                maxLength: 10,
                regex: /^[A-Z]{2}[0-9]{8}$/,
            }, align: 'right'
        }, {
            xtype: 'datecolumn',
            text: "<b><font color=red>效期</font></b>",
            dataIndex: 'EXP_DATE',
            style: 'text-align:left',
            width: 90,
            format: 'Xmd',
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
            text: "<b><font color=red>批號</font></b>",
            dataIndex: 'LOT_NO',
            style: 'text-align:left',
            width: 100,
            editor: {
                xtype: 'textfield',
                maxLength: 20,
                emptyText: 'TMPLOT'
            }, align: 'left'
        }, {
            text: "<b><font color=red>折讓金額</font></b>",
            dataIndex: 'EXTRA_DISC_AMOUNT',
            style: 'text-align:left',
            width: 110,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/ // 用正規表示式限制可輸入內容
            }, align: 'right'
        }, {
            text: "<b><font color=red>備註</font></b>",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            width: 150,
            editor: {
                xtype: 'textfield',
                maxLength: 300
            }, align: 'left'
        }, {
            text: "現行單價小計",
            dataIndex: 'IN_CONT_AMT',
            style: 'text-align:left',
            width: 110, align: 'right'
        }, {
            text: "合約價小計",
            dataIndex: 'IN_PO_AMT',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "優惠價小計",
            dataIndex: 'IN_DISC_AMT',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "總訂購量",
            dataIndex: 'PO_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "已進貨量",
            dataIndex: 'DELI_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "合約價", // 訂單單價
            dataIndex: 'PO_PRICE',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "現行單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "已進總價",
            dataIndex: 'DELI_AMT',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "訂單最小單價",
            dataIndex: 'UPRICE',
            style: 'text-align:left',
            width: 110,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行最小單價",
            dataIndex: 'UPRICE_M',
            style: 'text-align:left',
            width: 110,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "優惠單價", // 訂單優惠合約單價
            dataIndex: 'DISC_CPRICE',
            style: 'text-align:left',
            width: 140,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行優惠單價",
            dataIndex: 'DISC_CPRICE_M',
            style: 'text-align:left',
            width: 140,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "訂單優惠最小單價",
            dataIndex: 'DISC_UPRICE',
            style: 'text-align:left',
            width: 140,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "現行優惠最小單價",
            dataIndex: 'DISC_UPRICE_M',
            style: 'text-align:left',
            width: 140,
            align: 'right',
            showOption: false,
            hidden: true
        }, {
            text: "病人姓名",
            dataIndex: 'CHINNAME',
            width: 80,
            sortable: true
        }, {
            text: "病歷號",
            dataIndex: 'CHARTNO',
            width: 80,
            sortable: true
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
                T11Rec = records.length;
                T11LastRec = records[0];
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
            items: [T2Query]
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
            items: [T21Query]
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
            width: 100,
            sortable: true
        }, {
            text: "借貨量",
            dataIndex: 'BW_SQTY',
            style: 'text-align:left',
            width: 80,
            align: 'right'
        }, {
            text: "進貨接收量",
            dataIndex: 'ACC_QTY',
            style: 'text-align:left',
            width: 110,
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
            dataIndex: 'STATUS_N',
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
        Ext.MessageBox.confirm('變更單價', '是否確定更改' + parPO_NO + ' ' + parMMCODE + '單價，從' + parPO_PRICE + '改為' + parM_CONTPRICE + '?', function (btn, text) {
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

    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "未進貨":
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
                itemId: 't11Grid',
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
                            items: [T1Grid]
                        }, {
                            region: 'north',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            split: true,
                            height: '60%',
                            items: [T11Grid]
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
                            //title: '明細',
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

    T1Query.getForm().findField('P0').focus();
});
