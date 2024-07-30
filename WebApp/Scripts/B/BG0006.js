Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1GetExcel = '../../../api/BG0006/Excel';
    var T2GetExcel = '../../../api/BG0006/Excel_2';
    var MatComboGet = '../../../api/BG0006/GetMatCombo';
    var reportUrl = '/Report/B/BG0006.aspx';
    var T1Name = "廠商進貨明細表";

    var M_CONTID;
    var CONTRACNO;

    var T1Rec = 0;
    var T1LastRec = null;

    // 物料類別清單
    var MatQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    function setMatComboData() {
        Ext.Ajax.request({
            url: MatComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var mat_cls = data.etts;
                    if (mat_cls.length > 0) {
                        for (var i = 0; i < mat_cls.length; i++) {
                            MatQueryStore.add({ VALUE: mat_cls[i].VALUE, TEXT: mat_cls[i].TEXT });
                        }
                    }
                    T1Query.getForm().findField('P0').setValue("02");
                    T2Query.getForm().findField('P20').setValue("02");
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setMatComboData();
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P5',
        name: 'P5',
        fieldLabel: '院內碼(品項名稱)',
        labelAlign: 'right',
        labelWidth: 100,
        width: 300,
        limit: 200, //限制一次最多顯示10筆
        //fieldCls: 'required',
        //allowBlank: false,
        queryUrl: '/api/AA0006/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });
    var T2QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P25',
        name: 'P25',
        fieldLabel: '院內碼(品項名稱)',
        labelAlign: 'right',
        labelWidth: 100,
        width: 300,
        limit: 200, //限制一次最多顯示10筆
        //fieldCls: 'required',
        //allowBlank: false,
        queryUrl: '/api/AA0006/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T2Query.getForm().findField('P20').getValue()  //P20:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 70,
                    width: 180,
                    store: MatQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    allowBlank: false,
                    listeners: {
                        change: function () {
                            T1Grid.down('#t1export').setDisabled(true);
                            //T1Grid.down('#t1print').setDisabled(true);
                            //if (T1Query.getForm().findField('P0').getValue() == '02') {
                            //    T1Query.getForm().findField('P2').setDisabled(false);
                            //    T1Query.getForm().findField('P3').setDisabled(true);
                            //}
                            //else if (T1Query.getForm().findField('P0').getValue() == '01') {
                            //    T1Query.getForm().findField('P2').setDisabled(true);
                            //    T1Query.getForm().findField('P3').setDisabled(false);
                            //}
                            //else {
                            //    T1Query.getForm().findField('P2').setDisabled(true);
                            //    T1Query.getForm().findField('P3').setDisabled(true);
                            //}
                        }
                    },
                    editable: false,
                },
                {
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '進貨日期',
                    labelWidth: 70,
                    width: 150,
                    vtype: 'dateRange',
                    dateRange: { end: 'P4' },
                    fieldCls: 'required',
                    allowBlank: false,
                    editable: false,
                    listeners: {
                        change: function () {
                            T1Grid.down('#t1export').setDisabled(true);
                            //T1Grid.down('#t1print').setDisabled(true);
                        }
                    },
                    value: getTodayDate()
                },
                {
                    xtype: 'datefield',
                    id: 'P4',
                    name: 'P4',
                    fieldLabel: '至',
                    labelWidth: 20,
                    width: 100,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P1' },
                    fieldCls: 'required',
                    allowBlank: false,
                    editable: false,
                    listeners: {
                        change: function () {
                            T1Grid.down('#t1export').setDisabled(true);
                            //T1Grid.down('#t1print').setDisabled(true);
                        }
                    },
                    value: getTodayDate()
                },
                {
                    xtype: 'combo',
                    fieldLabel: '合約種類',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    labelWidth: 75,
                    width: 165,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners: {
                        change: function () {
                            T1Grid.down('#t1export').setDisabled(true);
                            //T1Grid.down('#t1print').setDisabled(true);
                        }
                    },
                    editable: false,
                    store: [
                        //{ VALUE: '', TEXT: '全部' },
                        { VALUE: '0', TEXT: '0 合約' },
                        { VALUE: '2', TEXT: '2 非合約' },
                        { VALUE: '3', TEXT: '3 小採' }
                    ],
                    value: '0',
                    //disabled: true
                }, T1QuryMMCode,
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 0 0 30',
                    handler: function () {
                        if (T1Query.getForm().findField('P0').isValid() && T1Query.getForm().findField('P1').isValid() && T1Query.getForm().findField('P4').isValid()) {
                            T1Load();
                            msglabel('訊息區:');
                            T1Grid.down('#t1export').setDisabled(false);
                            //T1Grid.down('#t1print').setDisabled(false);
                        }
                        else {
                            Ext.Msg.alert('訊息', '<span style="color:red; font-weight:bold">物料分類</span>與<span style="color:red; font-weight:bold">進貨日期</span>為必填。');
                            msglabel('訊息區:物料分類與進貨日期為必填。');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                        //T1Query.getForm().findField('P2').setDisabled(true);
                        //T1Query.getForm().findField('P3').setDisabled(true);
                    }
                }
                ]
            }]
    });

    var T1Store = Ext.create('WEBAPP.store.BG0006VM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    //p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1export', text: '匯出', disabled: true,
                handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    p.push({ name: 'FN', value: today + '_廠商進貨明細表.xls' });
                    p.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'P2', value: T1Query.getForm().findField('P2').getValue() });
                    //p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'P4', value: T1Query.getForm().findField('P4').rawValue });
                    p.push({ name: 'P5', value: T1Query.getForm().findField('P5').rawValue });
                    if (T1Store.getCount() > 0) {
                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    }
                    else {
                        Ext.Msg.alert('訊息', '請先建立資料');
                    }
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
            },
            {
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
                width: 120
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E'
            },
            {
                text: "包裝單位",
                dataIndex: 'M_PURUN',
                width: 70
            },
            {
                text: "進貨數量",
                dataIndex: 'ACC_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "合約價",
                dataIndex: 'PO_PRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 70
            },
            //{
            //text: "訂單數量",
            //dataIndex: 'PO_QTY',
            //align: 'right', // Right align the contents
            //style: 'text-align:left', // Keep left align for Header
            //width: 70
            //},
            //{
            //    text: "進貨金額",
            //    dataIndex: 'PO_AMT',
            //    align: 'right', // Right align the contents
            //    style: 'text-align:left', // Keep left align for Header
            //    width: 80
            //},
            {
                text: "優惠合約價",
                dataIndex: 'DISC_CPRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            //{
            //    text: "支付金額",
            //    dataIndex: 'PAY_AMOUNT',
            //    align: 'right', // Right align the contents
            //    style: 'text-align:left', // Keep left align for Header
            //    width: 80
            //},
            {
                text: "折讓比",
                dataIndex: 'M_DISCPERC',
                align: 'right',
                width: 60
            },
            //{
            //    text: "折讓金額",
            //    dataIndex: 'DISC_AMOUNT',
            //    align: 'right', // Right align the contents
            //    style: 'text-align:left', // Keep left align for Header
            //    width: 80
            //},
            {
                text: "進貨日期",
                dataIndex: 'ACC_TIME',
                width: 80
            },
            {
                text: "廠商碼",
                dataIndex: 'AGEN_NO',
                width: 60
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 200
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 80
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 80
            },
            {
                text: "衛署證號",
                dataIndex: 'M_PHCTNCO',
                width: 140
            },
            {
                text: "聯標否",
                dataIndex: 'M_MATID',
                width: 60
            },
            {
                header: "",
                flex: 1
            }],
        listeners: {
            click: {
                element: 'el',

            },
            selectionchange: function (model, records) {

            }
        },
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    if (T1Store.getCount() > 0) {
                        T1Grid.down('#t1export').setDisabled(false);
                        //T1Grid.down('#t1print').setDisabled(false);
                    }
                    else {
                        T1Grid.down('#t1export').setDisabled(true);
                        //T1Grid.down('#t1print').setDisabled(true);
                    }
                }
            }
        }
    });
    /*  彙總  T2Query*/
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP21',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P20',
                    id: 'P20',
                    enforceMaxLength: true,
                    labelWidth: 70,
                    width: 180,
                    store: MatQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    allowBlank: false,
                    listeners: {
                        change: function () {
                            T2Grid.down('#t2export').setDisabled(true);
                            T2Grid.down('#t2print').setDisabled(true);
                        }
                    },
                    editable: false,
                },
                {
                    xtype: 'datefield',
                    id: 'P21',
                    name: 'P21',
                    fieldLabel: '進貨日期',
                    labelWidth: 70,
                    width: 150,
                    vtype: 'dateRange',
                    dateRange: { end: 'P24' },
                    fieldCls: 'required',
                    allowBlank: false,
                    editable: false,
                    listeners: {
                        change: function () {
                            T2Grid.down('#t2export').setDisabled(true);
                            T2Grid.down('#t2print').setDisabled(true);
                        }
                    },
                    value: getTodayDate()
                },
                {
                    xtype: 'datefield',
                    id: 'P24',
                    name: 'P24',
                    fieldLabel: '至',
                    labelWidth: 20,
                    width: 100,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P21' },
                    fieldCls: 'required',
                    allowBlank: false,
                    editable: false,
                    listeners: {
                        change: function () {
                            T2Grid.down('#t2export').setDisabled(true);
                            T2Grid.down('#t2print').setDisabled(true);
                        }
                    },
                    value: getTodayDate()
                },
                {
                    xtype: 'combo',
                    fieldLabel: '合約種類',
                    name: 'P22',
                    id: 'P22',
                    enforceMaxLength: true,
                    labelWidth: 75,
                    width: 165,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners: {
                        change: function () {
                            T2Grid.down('#t2export').setDisabled(true);
                            T2Grid.down('#t2print').setDisabled(true);
                        }
                    },
                    editable: false,
                    store: [
                        //{ VALUE: '', TEXT: '全部' },
                        { VALUE: '0', TEXT: '0 合約' },
                        { VALUE: '2', TEXT: '2 非合約' },
                        { VALUE: '3', TEXT: '3 小採' }
                    ],
                    value: '0',
                    //disabled: true
                }, T2QuryMMCode,
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 0 0 30',
                    handler: function () {
                        
                        if (T2Query.getForm().findField('P20').isValid() && T2Query.getForm().findField('P21').isValid() && T2Query.getForm().findField('P24').isValid()) {
                            T2Load();
                            msglabel('訊息區:');
                            T2Grid.down('#t2export').setDisabled(false);
                            T2Grid.down('#t2print').setDisabled(false);
                        }
                        else {
                            Ext.Msg.alert('訊息', '<span style="color:red; font-weight:bold">物料分類</span>與<span style="color:red; font-weight:bold">進貨日期</span>為必填。');
                            msglabel('訊息區:物料分類與進貨日期為必填。');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P20').focus();
                        msglabel('訊息區:');
                        //T2Query.getForm().findField('P22').setDisabled(true);
                        //T2Query.getForm().findField('p23').setDisabled(true);
                    }
                }
                ]
            }]
    });

    var T2Store = Ext.create('WEBAPP.store.BG0006_2', {
        listeners: {
            beforeload: function (store, options) {
                
                var np = {
                    P20: T2Query.getForm().findField('P20').getValue(),
                    P21: T2Query.getForm().findField('P21').getValue(),
                    P22: T2Query.getForm().findField('P22').getValue(),
                    //p23: T2Query.getForm().findField('p23').getValue(),
                    P24: T2Query.getForm().findField('P24').getValue(),
                    P25: T2Query.getForm().findField('P25').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Tool.moveFirst();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't2print', text: '列印', disabled: true, handler: function () {
                    if (T2Store.getCount() > 0)
                        showReport_2();
                    else
                        Ext.Msg.alert('訊息', '請先建立資料');
                }
            }, {
                itemId: 't2export', text: '匯出', disabled: true,
                handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    
                    p.push({ name: 'FN', value: today + '_廠商進貨明細表.xls' });
                    p.push({ name: 'P20', value: T2Query.getForm().findField('P20').getValue() });
                    p.push({ name: 'P21', value: T2Query.getForm().findField('P21').rawValue });
                    p.push({ name: 'P22', value: T2Query.getForm().findField('P22').getValue() });
                    //p.push({ name: 'p23', value: T2Query.getForm().findField('p23').getValue() });
                    p.push({ name: 'P24', value: T2Query.getForm().findField('P24').rawValue });
                    p.push({ name: 'P25', value: T2Query.getForm().findField('P25').rawValue });
                    if (T2Store.getCount() > 0) {
                        PostForm(T2GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    }
                    else {
                        Ext.Msg.alert('訊息', '請先建立資料');
                    }
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E'
            },
            {
                text: "包裝單位",
                dataIndex: 'M_PURUN',
                width: 70
            },
            {
                text: "進貨數量",
                dataIndex: 'ACC_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "合約價",
                dataIndex: 'PO_PRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 70
            },
            //{
            //text: "訂單數量",
            //dataIndex: 'PO_QTY',
            //align: 'right', // Right align the contents
            //style: 'text-align:left', // Keep left align for Header
            //width: 70
            //},
            {
                text: "進貨金額",
                dataIndex: 'PO_AMT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "優惠合約價",
                dataIndex: 'DISC_CPRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "支付金額",
                dataIndex: 'PAY_AMOUNT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "折讓比",
                dataIndex: 'M_DISCPERC',
                align: 'right',
                width: 60
            },
            {
                text: "折讓金額",
                dataIndex: 'DISC_AMOUNT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "進貨日期",
                dataIndex: 'ACC_TIME',
                width: 80
            },
            //{
            //    text: "訂單編號",
            //    dataIndex: 'PO_NO',
            //    width: 150
            //},
            {
                text: "廠商碼",
                dataIndex: 'AGEN_NO',
                width: 60
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 200
            },
            {
                text: "衛署證號",
                dataIndex: 'M_PHCTNCO',
                width: 140
            },
            {
                text: "聯標否",
                dataIndex: 'M_MATID',
                width: 60
            },
            //{
            //    text: "合約碼",
            //    dataIndex: 'CONTRACNO',
            //    width: 60
            //},
            {
                header: "",
                flex: 1
            }],
        listeners: {
            click: {
                element: 'el',

            },
            selectionchange: function (model, records) {

            }
        },
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    if (T2Store.getCount() > 0) {
                        T2Grid.down('#t2export').setDisabled(false);
                        T2Grid.down('#t2print').setDisabled(false);
                    }
                    else {
                        T2Grid.down('#t2export').setDisabled(true);
                        T2Grid.down('#t2print').setDisabled(true);
                    }
                }
            }
        }
    });
    /**  T2Query end     */
    function showReport_2() {
        if (!win) {
            
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?MAT_CLASS=' + T2Query.getForm().findField('P20').getValue()
                + '&ACC_TIME_B=' + T2Query.getForm().findField('P21').rawValue
                + '&M_CONTID=' + T2Query.getForm().findField('P22').getValue()
                //+ '&CONTRACNO=' + T1Query.getForm().findField('P3').getValue()
                + '&ACC_TIME_E=' + T2Query.getForm().findField('P24').rawValue
                + '&MAT_CLASS_NAME=' + T2Query.getForm().findField('P20').rawValue
                + '&M_CONTID_NAME=' + T2Query.getForm().findField('P22').rawValue
                + '&MMCODE=' + T2Query.getForm().findField('P25').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義畫面 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        var TATabs = Ext.widget('tabpanel', {
            listeners: {
                tabchange: function (tabpanel, newCard, oldCard) {
                    switch (newCard.title) {
                        case "進貨彙總":
                            //T1QueryForm.getForm().findField('P0').focus();
                            break;
                        case "進貨明細":
                            //T2QueryForm.getForm().findField('P4').focus();
                            //T1QueryForm.getForm().findField('P0').clearInvalid();
                            //T1QueryForm.getForm().findField('P1').clearInvalid();
                            //T1QueryForm.getForm().findField('P2').clearInvalid();
                            //T1QueryForm.getForm().findField('P3').clearInvalid();
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
                itemId: 't2TAB',
                title: '進貨彙總',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T2Grid]
            }, {
                itemId: 't1TAB',
                title: '進貨明細',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Grid]
            }]
        });
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
            itemId: 't1Form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [TATabs]
        }
        ]
    });

});
