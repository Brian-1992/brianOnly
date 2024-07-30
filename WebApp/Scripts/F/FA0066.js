Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = '成本單位明細報表'
    var MATComboGet = '../../../api/FA0066/GetMatCombo';
    var YMComboGet = '../../../api/FA0066/GetYMCombo';
    var reportUrl = '/Report/F/FA0066.aspx';
    var T1GetExcel = '../../../api/FA0066/ExportExcel';

    var tempCLS;
    var parCLS = '';
    var printCLS;
    var T1LastRec = null;
    var getStoreIDRadio;
    var getInidFlagRadio;
    var tmpYM;
    var getMatName;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    // 物料類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 成本年月清單
    var YMQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 是否清單
    var YNStore = Ext.create('Ext.data.Store', {
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
    
    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P5',
        fieldLabel: '',
        padding: '4 4 4 4',
        width: 100,
        disabled: menuLink != "AA0164",

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0066/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    function setMATComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var mat_cls = data.etts;
                    tempCLS = mat_cls;
                    if (mat_cls.length > 0) {
                        MATQueryStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < mat_cls.length; i++) {
                            MATQueryStore.add({ VALUE: mat_cls[i].VALUE, TEXT: mat_cls[i].TEXT });
                        }
                    }
                    T1Query.getForm().findField('P0').setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setMATComboData();

    function setYMComboData() {
        Ext.Ajax.request({
            url: YMComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var set_ym = data.etts;
                    if (set_ym.length > 0) {
                        for (var i = 0; i < set_ym.length; i++) {
                            YMQueryStore.add({ VALUE: set_ym[i].VALUE, TEXT: set_ym[i].TEXT });
                            if (i == 0) {
                                tmpYM = set_ym[i];
                            }
                        }
                    }
                    T1Query.getForm().findField('P1').setValue(tmpYM);  //預設第一筆最近年月
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setYMComboData();

    function setYNComboData() {
        Ext.Ajax.request({
            url: '/api/FA0066/GetYNCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var datas = data.etts;
                    YNStore.add({ VALUE: '', TEXT: '' });
                    if (datas.length > 0) {
                        for (var i = 0; i < datas.length; i++) {
                            YNStore.add({ VALUE: datas[i].VALUE, TEXT: datas[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setYNComboData();
   
    var T1Store = Ext.create('WEBAPP.store.FA0066VM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: false,
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    p9: T1Query.getForm().findField('P9').checked ? "Y" : "N",
                    p10: T1Query.getForm().findField('P10').getValue(),
                    p11: T1Query.getForm().findField('P11').getValue(),
                    p12: T1Query.getForm().findField('P12').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    //搜尋院內碼
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P7',
        id: 'P7',
        fieldLabel: '院內碼',
        allowBlank: true,
        labelWidth: 60,
        width: 150,

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T1Query.getForm();
            if (f.findField('P0').getValue() != "") {
                return {
                    MMCODE: f.findField('P7').getValue(),
                    MAT_CLASS: f.findField('P0').getValue(),
                    clsALL: false
                };
            }
            else {
                for (var i = 0; i < tempCLS.length; i++) {
                    parCLS += tempCLS[i].VALUE + ',';
                };
                return {
                    MMCODE: f.findField('P7').getValue(),
                    MAT_CLASS: parCLS,
                    clsALL: true
                };
            }
        },

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0066/GetMMCodeComboQ',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Query.getForm();
                if (r.get('P7') !== '') {
                    f.findField('P7').setValue(r.get('MMCODE'));
                }
            }
        }
    });

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
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '成本年月',
                            name: 'P1',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 140,
                            padding: '4 4 4 0',
                            store: YMQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);
                                }
                            },
                            editable: false
                        },
                        {
                            xtype: 'radiogroup',
                            fieldLabel: '單位選項',
                            name: 'P2',
                            padding: '4 4 4 0',
                            width: menuLink == "AA0164" ? 150 : 534,
                            items: [
                                {
                                    //checked: true,
                                    width: menuLink == "AA0164" ? 1 : 90,
                                    boxLabel: '應盤點單位',
                                    name: 'P2',
                                    id: 'P2_0',
                                    inputValue: '0',
                                    hidden: menuLink == "AA0164",
                                }, {
                                    width: menuLink == "AA0164" ? 1 : 90,
                                    boxLabel: '行政科室',
                                    name: 'P2',
                                    id: 'P2_1',
                                    inputValue: '1',
                                    hidden: menuLink == "AA0164",
                                }, {
                                    width: menuLink == "AA0164" ? 1 : 100,
                                    boxLabel: '財務獨立單位',
                                    name: 'P2',
                                    id: 'P2_2',
                                    inputValue: '2',
                                    hidden: menuLink == "AA0164",
                                }, {
                                    width: menuLink == "AA0164" ? 1 : 60,
                                    boxLabel: '全院',
                                    name: 'P2',
                                    id: 'P2_3',
                                    inputValue: '3',
                                    listeners: {
                                        change: function (radioGroup, newValue, oldValue) {
                                            Ext.getCmp('t1print').setDisabled(true);
                                            Ext.getCmp('t1export').setDisabled(true);
                                        }
                                    },
                                    hidden: menuLink == "AA0164",
                                }, {
                                    checked: true,
                                    width: 70,
                                    boxLabel: '單位科別',
                                    name: 'P2',
                                    id: 'P2_4',
                                    inputValue: '4'
                                }
                            ],
                            listeners: {
                                change: function (radiogroup, newValue, oldValue) {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);

                                    switch (newValue.P2) {
                                        case '3':   //選擇全院
                                            {
                                                T1Query.getForm().findField('P5').setDisabled(true);
                                                T1Query.getForm().findField('P3_3').setDisabled(false);
                                                break;
                                            }
                                        case '4':   //選擇單科
                                            {
                                                T1Query.getForm().findField('P5').setDisabled(false);
                                                T1Query.getForm().findField('P3_3').setDisabled(false);
                                                break;
                                            }
                                        default:    //其他
                                            {
                                                if (T1Query.getForm().findField('P3_3').getGroupValue() == '3') {
                                                    T1Query.getForm().findField('P3_3').setValue("0");
                                                }

                                                if (menuLink == "AA0164") {
                                                    T1Query.getForm().findField('P5').setDisabled(false);
                                                    T1Query.getForm().findField('P3_3').setDisabled(false);
                                                }
                                                else {
                                                    T1Query.getForm().findField('P5').setDisabled(true);
                                                    T1Query.getForm().findField('P3_3').setDisabled(true);
                                                }
                                            }
                                    }
                                }
                            }
                        },
                        wh_NoCombo,
                        {
                            name: 'P6',
                            xtype: 'hidden'
                        }, {
                            fieldLabel: '庫存小於0',
                            name: 'P06',
                            xtype: 'checkboxfield',
                            labelWidth: 80, width: 100,
                            labelAlign: 'right',
                            listeners: {
                                change: function (checkbox, newValue, oldValue, eOpts) {
                                    if (checkbox.checked) {
                                        T1Query.getForm().findField('P6').setValue("Y");
                                    } else {
                                        T1Query.getForm().findField('P6').setValue("N");
                                    }
                                }
                            }
                        },
                        {
                            name: 'P8',
                            xtype: 'hidden'
                        }, {
                            fieldLabel: '消耗量小於0',
                            name: 'P08',
                            xtype: 'checkboxfield',
                            labelWidth: 90, width: 110,
                            labelAlign: 'right',
                            listeners: {
                                change: function (checkbox, newValue, oldValue, eOpts) {
                                    if (checkbox.checked) {
                                        T1Query.getForm().findField('P8').setValue("Y");
                                    } else {
                                        T1Query.getForm().findField('P8').setValue("N");
                                    }
                                }
                            }
                        },
                        {
                            fieldLabel: '期初小於0',
                            name: 'P9',
                            xtype: 'checkboxfield',
                            labelWidth: 80, width: 110,
                            labelAlign: 'right'
                        }
                    ]
                }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    name: 'P0',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 140,
                    padding: '4 4 4 0',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners: {
                        change: function () {
                            Ext.getCmp('t1print').setDisabled(true);
                            Ext.getCmp('t1export').setDisabled(true);
                        }
                    },
                    editable: false
                }, {
                    //院內碼
                    xtype: 'container',
                    layout: 'hbox',
                    padding: '4 4 0 0',
                    id: 'mmcodeComboSet',
                    items: [
                        mmCodeCombo
                    ]
                },
                {
                    xtype: 'radiogroup',
                    name: 'P3',
                    padding: '4 4 4 0',
                    width: 600,
                    fieldLabel: '是否庫備',
                    items: [
                        {
                            checked: true,
                            boxLabel: '不區分',
                            name: 'P3',
                            id: 'P3_0',
                            inputValue: '0',
                            width: 70
                        },
                        {
                            boxLabel: '庫備品',
                            name: 'P3', id: 'P3_1',
                            inputValue: '1',
                            width: 70
                        },
                        {
                            boxLabel: '非庫備品',
                            name: 'P3',
                            id: 'P3_2',
                            inputValue: '2',
                            width: 80,
                        },
                        {
                            boxLabel: '單價 > 5000 且週轉率 < 1 (僅限全院和單科)',
                            name: 'P3',
                            id: 'P3_3',
                            inputValue: '3',
                            width: 500,
                            disabled: menuLink != "AA0164",
                        },
                    ],
                    listeners: {
                        change: function (radioGroup, newValue, oldValue) {
                            Ext.getCmp('t1print').setDisabled(true);
                            Ext.getCmp('t1export').setDisabled(true);


                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '4 0 4 90',
                    handler: function () {
                        var SET_YM = T1Query.getForm().findField('P1').rawValue;

                        if (SET_YM != null && SET_YM != '') {
                            msglabel('訊息區:');
                            T1Load();
                            Ext.getCmp('t1print').setDisabled(false);
                            Ext.getCmp('t1export').setDisabled(false);
                        }
                        else {
                            Ext.MessageBox.alert('提示', '請選擇<span style="color:red; font-weight:bold">成本年月</span>再進行查詢。');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    margin: '4 4 4 4',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0

                        Ext.getCmp('t1print').setDisabled(true);
                        Ext.getCmp('t1export').setDisabled(true);
                    }
                }
            ]
            },{
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '院內碼是否作廢',
                        name: 'P10',
                        enforceMaxLength: true,
                        labelWidth: 110,
                        width: 160,
                        padding: '4 4 4 0',
                        store: YNStore,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        //fieldCls: 'required',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        editable: false
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '是否寄售',
                        name: 'P11',
                        enforceMaxLength: true,
                        labelWidth: 60,
                        width: 140,
                        padding: '4 4 4 0',
                        store: YNStore,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        //fieldCls: 'required',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        editable: false,
                        hidden: menuLink == "AA0164",
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '庫房是否作廢',
                        name: 'P12',
                        enforceMaxLength: true,
                        labelWidth: 90,
                        width: 140,
                        padding: '4 4 4 0',
                        store: YNStore,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        //fieldCls: 'required',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        editable: false
                    },
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
                id: 't1print', text: '列印', disabled: true, handler: function () {
                    if (T1Store.getCount() > 0)
                        showReport();
                    else
                        Ext.Msg.alert('訊息', '請先建立報表資料。');
                }
            },
            {
                id: 't1export', text: '匯出', disabled: true, handler: function () {

                    showWin3();
                    
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
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
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "單位代碼",
                dataIndex: 'WH_NO',
                width: 70
            },
            {
                text: "單位名稱",
                dataIndex: 'WH_NAME',
                width: 180
            },
            {
                text: "成本年月",
                dataIndex: 'DATA_YM',
                width: 70
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 300
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 300
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70
            },
            {
                text: "期初數量",
                dataIndex: 'P_INV_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "進貨數量",
                dataIndex: 'IN_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "消耗數量",
                dataIndex: 'OUT_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "結存數量",
                dataIndex: 'INV_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "盤盈虧數量",
                dataIndex: 'INVENTORYQTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "期初成本",
                dataIndex: 'SUM1',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "進貨成本",
                dataIndex: 'SUM2',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "消耗金額",
                dataIndex: 'SUM3',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "期末成本",
                dataIndex: 'SUMTOT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "差異金額",
                dataIndex: 'D_AMT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "盤盈虧金額",
                dataIndex: 'SUM4',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "基準量",
                dataIndex: 'HIGH_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "庫存單價",
                dataIndex: 'DISC_CPRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "超量",
                dataIndex: 'SUB_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "超量金額",
                dataIndex: 'SUB_PRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "週轉率",
                dataIndex: 'TURNOVER',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "進貨單價",
                dataIndex: 'IN_PRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "採購途徑",
                dataIndex: 'M_CONTID',
                width: 100
            },
            {
                text: "庫備否",
                dataIndex: 'M_STOREID',
                width: 100
            },
            {
                text: "給付類別",
                dataIndex: 'M_PAYKIND',
                width: 100
            },
            {
                text: "是否半消耗品",
                dataIndex: 'M_CONSUMID',
                width: 100
            },
            {
                text: "是否扣庫",
                dataIndex: 'M_TRNID',
                width: 100
            },
            {
                text: "是否計價",
                dataIndex: 'M_PAYID',
                width: 100
            },
            {
                text: "效期",
                dataIndex: 'EXPLOT',
                width: 100
            },
            {
                text: "調撥",
                dataIndex: 'TRN_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "繳回",
                dataIndex: 'BAK_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "調帳",
                dataIndex: 'ADJ_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "退貨",
                dataIndex: 'REJ_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "報廢",
                dataIndex: 'DIS_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "換貨",
                dataIndex: 'EXG_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            },
            {
                text: "戰備換貨",
                dataIndex: 'MIL_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 100
            }, {
                text: "院內碼是否作廢",
                dataIndex: 'CANCEL_ID',
                width: 100
            }, {
                text: "是否寄售",
                dataIndex: 'IS_SOURCE_C',
                width: 100
            }, {
                text: "庫房是否作廢",
                dataIndex: 'WHNO_CANCEL',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    //T1Grid.down('#t1print').setDisabled(true);
                }
            }
        },
        listeners: {

            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];

                if (T1LastRec != null) {
                    Ext.getCmp('t1print').setDisabled(false);
                    Ext.getCmp('t1export').setDisabled(false);
                }
            }
        }
    });

    function showReport() {
        if (!win) {
            var p4 = '';
            var P2RadioName = '';
            var P3RadioName = '';
            if (T1Query.getForm().findField('P0').getValue() !== '') {
                p4 = false;
                printCLS = T1Query.getForm().findField('P0').getValue();
                getMatName = T1Query.getForm().findField('P0').rawValue.substr(3);
            }
            else {
                p4 = true;
                printCLS = parCLS;
                getMatName = "全部";
            };

            //取得單位選項的Radio
            if (T1Query.getForm().findField('P2_0').checked) {
                getInidFlagRadio = '0';
                P2RadioName = T1Query.getForm().findField('P2_0').boxLabel;
            }
            else if (T1Query.getForm().findField('P2_1').checked) {
                getInidFlagRadio = '1';
                P2RadioName = T1Query.getForm().findField('P2_1').boxLabel;
            }
            else if (T1Query.getForm().findField('P2_2').checked) {
                getInidFlagRadio = '2';
                P2RadioName = T1Query.getForm().findField('P2_2').boxLabel;
            }
            else if (T1Query.getForm().findField('P2_3').checked) {
                getInidFlagRadio = '3';
                P2RadioName = T1Query.getForm().findField('P2_3').boxLabel;
            }
            else if (T1Query.getForm().findField('P2_4').checked) {
                getInidFlagRadio = '4';
                P2RadioName = T1Query.getForm().findField('P2_4').boxLabel;
            }

            //取得是否庫備Radio
            if (T1Query.getForm().findField('P3_0').checked) {
                getStoreIDRadio = '0';
                P3RadioName = T1Query.getForm().findField('P3_0').boxLabel;
            }
            else if (T1Query.getForm().findField('P3_1').checked) {
                getStoreIDRadio = '1';
                P3RadioName = T1Query.getForm().findField('P3_1').boxLabel;
            }
            else if (T1Query.getForm().findField('P3_2').checked) {
                getStoreIDRadio = '2';
                P3RadioName = T1Query.getForm().findField('P3_2').boxLabel;
            }
            else if (T1Query.getForm().findField('P3_3').checked) {
                getStoreIDRadio = '3';
                P3RadioName = T1Query.getForm().findField('P3_3').boxLabel;
            }

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?MAT_CLASS=' + T1Query.getForm().findField('P0').getValue()
                    + '&SET_YM=' + T1Query.getForm().findField('P1').rawValue
                    + '&INIDFLAG=' + getInidFlagRadio
                    + '&M_STOREID=' + getStoreIDRadio
                    + '&clsALL=' + false
                    + '&WH_NO=' + T1Query.getForm().findField('P5').rawValue
                    + '&IS_INV_MINUS=' + T1Query.getForm().findField('P6').getValue()
                    + '&MMCODE=' + T1Query.getForm().findField('P7').getValue()
                    + '&IS_OUT_MINUS=' + T1Query.getForm().findField('P8').getValue()
                    + '&getMatName=' + getMatName
                    + '&getInidFlagName=' + P2RadioName
                    + '&getM_StoreIDName=' + P3RadioName
                + '&IS_PMNQTY_MINUS=' + (T1Query.getForm().findField('P9').checked ? "Y" : "N")
                + '&cancel_id=' + (T1Query.getForm().findField('P10').getValue())
                + '&is_source_c=' + (T1Query.getForm().findField('P11').getValue())
                + '&whno_cancel=' + (T1Query.getForm().findField('P12').getValue())
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }
    var T3FormlabelWidth = 100;

    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        height: 40,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [
                {
                    xtype: 'button',
                    text: '匯出',
                    handler: function () {
                        var today = getTodayDate();
                        var p = new Array();
                        var p4 = '';
                        var excelP2Radio;
                        var excelP3Radio;

                        //取得單位選項Radio
                        if (T1Query.getForm().findField('P2_0').checked) {
                            excelP2Radio = '0';
                        }
                        else if (T1Query.getForm().findField('P2_1').checked) {
                            excelP2Radio = '1';
                        }
                        else if (T1Query.getForm().findField('P2_2').checked) {
                            excelP2Radio = '2';
                        }
                        else if (T1Query.getForm().findField('P2_3').checked) {
                            excelP2Radio = '3';
                        }
                        else if (T1Query.getForm().findField('P2_4').checked) {
                            excelP2Radio = '4';
                        }

                        //取得是否庫備Radio
                        if (T1Query.getForm().findField('P3_0').checked) {
                            excelP3Radio = '0';
                        }
                        else if (T1Query.getForm().findField('P3_1').checked) {
                            excelP3Radio = '1';
                        }
                        else if (T1Query.getForm().findField('P3_2').checked) {
                            excelP3Radio = '2';
                        }
                        else if (T1Query.getForm().findField('P3_3').checked) {
                            excelP3Radio = '3';
                        }

                        p.push({ name: 'FN', value: today + '_成本單位明細報表.xlsx' });
                        p.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                        
                        p.push({ name: 'P2', value: excelP2Radio });
                        p.push({ name: 'P3', value: excelP3Radio });
                        p.push({ name: 'P4', value: false });
                        p.push({ name: 'P5', value: T1Query.getForm().findField('P5').getValue() });
                        p.push({ name: 'P6', value: T1Query.getForm().findField('P6').getValue() });
                        p.push({ name: 'P7', value: T1Query.getForm().findField('P7').getValue() });
                        p.push({ name: 'P8', value: T1Query.getForm().findField('P8').getValue() });
                        p.push({ name: 'P9', value: T1Query.getForm().findField('P9').getValue() });
                        p.push({ name: 'P10', value: T1Query.getForm().findField('P10').getValue() });
                        p.push({ name: 'P11', value: T1Query.getForm().findField('P11').getValue() });
                        p.push({ name: 'P12', value: T1Query.getForm().findField('P12').getValue() });
                        p.push({ name: 'T3P1', value: T3Form.getForm().findField('T3P1').getValue() });
                        p.push({ name: 'T3P2', value: T3Form.getForm().findField('T3P2').getValue() });
                        p.push({ name: 'T3P3', value: T3Form.getForm().findField('T3P3').getValue() });
                        p.push({ name: 'T3P4', value: T3Form.getForm().findField('T3P4').getValue() });
                        p.push({ name: 'T3P5', value: T3Form.getForm().findField('T3P5').getValue() });
                        p.push({ name: 'T3P6', value: T3Form.getForm().findField('T3P6').getValue() });
                        p.push({ name: 'T3P7', value: T3Form.getForm().findField('T3P7').getValue() });
                        p.push({ name: 'T3P8', value: T3Form.getForm().findField('T3P8').getValue() });
                        p.push({ name: 'T3P9', value: T3Form.getForm().findField('T3P9').getValue() });
                        p.push({ name: 'T3P10', value: T3Form.getForm().findField('T3P10').getValue() });
                        p.push({ name: 'T3P11', value: T3Form.getForm().findField('T3P11').getValue() });
                        p.push({ name: 'T3P12', value: T3Form.getForm().findField('T3P12').getValue() });
                        p.push({ name: 'T3P13', value: T3Form.getForm().findField('T3P13').getValue() });
                        p.push({ name: 'T3P14', value: T3Form.getForm().findField('T3P14').getValue() });
                        p.push({ name: 'T3P15', value: T3Form.getForm().findField('T3P15').getValue() });
                        p.push({ name: 'T3P16', value: T3Form.getForm().findField('T3P16').getValue() });
                        p.push({ name: 'T3P17', value: T3Form.getForm().findField('T3P17').getValue() });
                        p.push({ name: 'T3P18', value: T3Form.getForm().findField('T3P18').getValue() });
                        p.push({ name: 'T3P19', value: T3Form.getForm().findField('T3P19').getValue() });
                        p.push({ name: 'T3P20', value: T3Form.getForm().findField('T3P20').getValue() });
                        p.push({ name: 'T3P21', value: T3Form.getForm().findField('T3P21').getValue() });
                        p.push({ name: 'T3P22', value: T3Form.getForm().findField('T3P22').getValue() });
                        p.push({ name: 'T3P23', value: T3Form.getForm().findField('T3P23').getValue() });
                        p.push({ name: 'T3P24', value: T3Form.getForm().findField('T3P24').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P25', value: T3Form.getForm().findField('T3P25').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P26', value: T3Form.getForm().findField('T3P26').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P27', value: T3Form.getForm().findField('T3P27').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P28', value: T3Form.getForm().findField('T3P28').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P29', value: T3Form.getForm().findField('T3P29').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P30', value: T3Form.getForm().findField('T3P30').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P31', value: T3Form.getForm().findField('T3P31').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P32', value: T3Form.getForm().findField('T3P32').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P33', value: T3Form.getForm().findField('T3P33').checked ? 'Y' : 'N' });
                        p.push({ name: 'T3P34', value: T3Form.getForm().findField('T3P34').checked ? 'Y' : 'N' });
                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    }
            },
            {
                xtype: 'button', text: '取消', handler: hideWin3
            },
            {
                xtype: 'button', text: '全選', handler: function () {

                    T3Form.getForm().findField('T3P1').setValue('Y');
                    T3Form.getForm().findField('T3P2').setValue('Y');
                    T3Form.getForm().findField('T3P3').setValue('Y');
                    T3Form.getForm().findField('T3P4').setValue('Y');
                    T3Form.getForm().findField('T3P5').setValue('Y');
                    T3Form.getForm().findField('T3P6').setValue('Y');
                    T3Form.getForm().findField('T3P7').setValue('Y');
                    T3Form.getForm().findField('T3P8').setValue('Y');
                    T3Form.getForm().findField('T3P9').setValue('Y');
                    T3Form.getForm().findField('T3P10').setValue('Y');
                    T3Form.getForm().findField('T3P11').setValue('Y');
                    T3Form.getForm().findField('T3P12').setValue('Y');
                    T3Form.getForm().findField('T3P13').setValue('Y');
                    T3Form.getForm().findField('T3P14').setValue('Y');
                    T3Form.getForm().findField('T3P15').setValue('Y');
                    T3Form.getForm().findField('T3P16').setValue('Y');
                    T3Form.getForm().findField('T3P17').setValue('Y');
                    T3Form.getForm().findField('T3P18').setValue('Y');
                    T3Form.getForm().findField('T3P19').setValue('Y');
                    T3Form.getForm().findField('T3P20').setValue('Y');
                    T3Form.getForm().findField('T3P21').setValue('Y');
                    T3Form.getForm().findField('T3P22').setValue('Y');
                    T3Form.getForm().findField('T3P23').setValue('Y');
                    T3Form.getForm().findField('T3P24').setValue('Y');
                    Ext.getCmp('T3P01').setValue(true);
                    Ext.getCmp('T3P02').setValue(true);
                    Ext.getCmp('T3P03').setValue(true);
                    Ext.getCmp('T3P04').setValue(true);
                    Ext.getCmp('T3P05').setValue(true);
                    Ext.getCmp('T3P06').setValue(true);
                    Ext.getCmp('T3P07').setValue(true);
                    Ext.getCmp('T3P08').setValue(true);
                    Ext.getCmp('T3P09').setValue(true);
                    Ext.getCmp('T3P010').setValue(true);
                    Ext.getCmp('T3P011').setValue(true);
                    Ext.getCmp('T3P012').setValue(true);
                    Ext.getCmp('T3P013').setValue(true);
                    Ext.getCmp('T3P014').setValue(true);
                    Ext.getCmp('T3P015').setValue(true);
                    Ext.getCmp('T3P016').setValue(true);
                    Ext.getCmp('T3P017').setValue(true);
                    Ext.getCmp('T3P018').setValue(true);
                    Ext.getCmp('T3P019').setValue(true);
                    Ext.getCmp('T3P020').setValue(true);
                    Ext.getCmp('T3P021').setValue(true);
                    Ext.getCmp('T3P022').setValue(true);
                    Ext.getCmp('T3P023').setValue(true);
                    Ext.getCmp('T3P24').setValue(true);
                    Ext.getCmp('T3P25').setValue(true);
                    Ext.getCmp('T3P26').setValue(true);
                    Ext.getCmp('T3P27').setValue(true);
                    Ext.getCmp('T3P28').setValue(true);
                    Ext.getCmp('T3P29').setValue(true);
                    Ext.getCmp('T3P30').setValue(true);
                    Ext.getCmp('T3P31').setValue(true);
                    Ext.getCmp('T3P32').setValue(true);
                    Ext.getCmp('T3P33').setValue(true);
                    Ext.getCmp('T3P34').setValue(true);
                }
            },
            {
                xtype: 'button', text: '全不選', handler: function () {
                    
                    T3Form.getForm().findField('T3P1').setValue('N');
                    T3Form.getForm().findField('T3P2').setValue('N');
                    T3Form.getForm().findField('T3P3').setValue('N');
                    T3Form.getForm().findField('T3P4').setValue('N');
                    T3Form.getForm().findField('T3P5').setValue('N');
                    T3Form.getForm().findField('T3P6').setValue('N');
                    T3Form.getForm().findField('T3P7').setValue('N');
                    T3Form.getForm().findField('T3P8').setValue('N');
                    T3Form.getForm().findField('T3P9').setValue('N');
                    T3Form.getForm().findField('T3P10').setValue('N');
                    T3Form.getForm().findField('T3P11').setValue('N');
                    T3Form.getForm().findField('T3P12').setValue('N');
                    T3Form.getForm().findField('T3P13').setValue('N');
                    T3Form.getForm().findField('T3P14').setValue('N');
                    T3Form.getForm().findField('T3P15').setValue('N');
                    T3Form.getForm().findField('T3P16').setValue('N');
                    T3Form.getForm().findField('T3P17').setValue('N');
                    T3Form.getForm().findField('T3P18').setValue('N');
                    T3Form.getForm().findField('T3P19').setValue('N');
                    T3Form.getForm().findField('T3P20').setValue('N');
                    T3Form.getForm().findField('T3P21').setValue('N');
                    T3Form.getForm().findField('T3P22').setValue('N');
                    T3Form.getForm().findField('T3P23').setValue('N');
                    T3Form.getForm().findField('T3P24').setValue('N');
                    Ext.getCmp('T3P01').setValue(false);
                    Ext.getCmp('T3P02').setValue(false);
                    Ext.getCmp('T3P03').setValue(false);
                    Ext.getCmp('T3P04').setValue(false);
                    Ext.getCmp('T3P05').setValue(false);
                    Ext.getCmp('T3P06').setValue(false);
                    Ext.getCmp('T3P07').setValue(false);
                    Ext.getCmp('T3P08').setValue(false);
                    Ext.getCmp('T3P09').setValue(false);
                    Ext.getCmp('T3P010').setValue(false);
                    Ext.getCmp('T3P011').setValue(false);
                    Ext.getCmp('T3P012').setValue(false);
                    Ext.getCmp('T3P013').setValue(false);
                    Ext.getCmp('T3P014').setValue(false);
                    Ext.getCmp('T3P015').setValue(false);
                    Ext.getCmp('T3P016').setValue(false);
                    Ext.getCmp('T3P017').setValue(false);
                    Ext.getCmp('T3P018').setValue(false);
                    Ext.getCmp('T3P019').setValue(false);
                    Ext.getCmp('T3P020').setValue(false);
                    Ext.getCmp('T3P021').setValue(false);
                    Ext.getCmp('T3P022').setValue(false);
                    Ext.getCmp('T3P023').setValue(false);
                    Ext.getCmp('T3P24').setValue(false);
                    Ext.getCmp('T3P25').setValue(false);
                    Ext.getCmp('T3P26').setValue(false);
                    Ext.getCmp('T3P27').setValue(false);
                    Ext.getCmp('T3P28').setValue(false);
                    Ext.getCmp('T3P29').setValue(false);
                    Ext.getCmp('T3P30').setValue(false);
                    Ext.getCmp('T3P31').setValue(false);
                    Ext.getCmp('T3P32').setValue(false);
                    Ext.getCmp('T3P33').setValue(false);
                    Ext.getCmp('T3P34').setValue(false);
                }
            }
        ]
    });
    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side'
        },
        autoScroll: true,
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T3Query]
        }],
        items: [
            {
                name: 'T3P1',
                id: 'T3P1',
                xtype: 'hidden'
            }, {
                fieldLabel: '期初數量',
                name: 'T3P01',
                id: 'T3P01',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P1').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P1').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P2',
                id: 'T3P2',
                xtype: 'hidden'
            }, {
                fieldLabel: '進貨數量',
                name: 'T3P02',
                id: 'T3P02',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P2').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P2').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P3',
                id: 'T3P3',
                xtype: 'hidden'
            }, {
                fieldLabel: '消耗數量',
                name: 'T3P03',
                id: 'T3P03',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P3').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P3').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P4',
                id: 'T3P4',
                xtype: 'hidden'
            }, {
                fieldLabel: '結存數量',
                name: 'T3P04',
                id: 'T3P04',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P4').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P4').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P5',
                id: 'T3P5',
                xtype: 'hidden'
            }, {
                fieldLabel: '盤盈虧數量',
                name: 'T3P05',
                id: 'T3P05',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P5').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P5').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P6',
                id: 'T3P6',
                xtype: 'hidden'
            }, {
                fieldLabel: '期初成本',
                name: 'T3P06',
                id: 'T3P06',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P6').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P6').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P7',
                id: 'T3P7',
                xtype: 'hidden'
            }, {
                fieldLabel: '進貨成本',
                name: 'T3P07',
                id: 'T3P07',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P7').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P7').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P8',
                id: 'T3P8',
                xtype: 'hidden'
            }, {
                fieldLabel: '消耗金額',
                name: 'T3P08',
                id: 'T3P08',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P8').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P8').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P9',
                id: 'T3P9',
                xtype: 'hidden'
            }, {
                fieldLabel: '期末成本',
                name: 'T3P09',
                id: 'T3P09',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P9').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P9').setValue("N");
                        }
                    }
                }
            }, {
                fieldLabel: '差異金額',
                name: 'T3P34',
                id: 'T3P34',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                name: 'T3P10',
                id: 'T3P10',
                xtype: 'hidden'
            }, {
                fieldLabel: '盤盈虧金額',
                name: 'T3P010',
                id: 'T3P010',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P10').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P10').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P11',
                id: 'T3P11',
                xtype: 'hidden'
            }, {
                fieldLabel: '基準量',
                name: 'T3P011',
                id: 'T3P011',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P11').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P11').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P12',
                id: 'T3P12',
                xtype: 'hidden'
            }, {
                fieldLabel: '庫存單價',
                name: 'T3P012',
                id: 'T3P012',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P12').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P12').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P13',
                id: 'T3P13',
                xtype: 'hidden'
            }, {
                fieldLabel: '超量',
                name: 'T3P013',
                id: 'T3P013',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P13').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P13').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P14',
                id: 'T3P14',
                xtype: 'hidden'
            }, {
                fieldLabel: '超量金額',
                name: 'T3P014',
                id: 'T3P014',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P14').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P14').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P15',
                id: 'T3P15',
                xtype: 'hidden'
            }, {
                fieldLabel: '週轉率',
                name: 'T3P015',
                id: 'T3P015',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P15').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P15').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P16',
                id: 'T3P16',
                xtype: 'hidden'
            }, {
                fieldLabel: '進貨單價',
                name: 'T3P016',
                id: 'T3P016',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P16').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P16').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P17',
                id: 'T3P17',
                xtype: 'hidden'
            }, {
                fieldLabel: '小採否',
                name: 'T3P017',
                id: 'T3P017',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P17').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P17').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P18',
                id: 'T3P18',
                xtype: 'hidden'
            }, {
                fieldLabel: '庫備否',
                name: 'T3P018',
                id: 'T3P018',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P18').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P18').setValue("N");
                        }
                    }
                }
            } , {
                name: 'T3P19',
                id: 'T3P19',
                xtype: 'hidden'
            }, {
                fieldLabel: '給付類別',
                name: 'T3P019',
                id: 'T3P019',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P19').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P19').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P20',
                id: 'T3P20',
                xtype: 'hidden'
            }, {
                fieldLabel: '是否半消耗品',
                name: 'T3P020',
                id: 'T3P020',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P20').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P20').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P21',
                id: 'T3P21',
                xtype: 'hidden'
            }, {
                fieldLabel: '是否扣庫',
                name: 'T3P021',
                id: 'T3P021',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P21').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P21').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P22',
                id: 'T3P22',
                xtype: 'hidden'
            }, {
                fieldLabel: '是否計價',
                name: 'T3P022',
                id: 'T3P022',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P22').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P22').setValue("N");
                        }
                    }
                }
            }, {
                name: 'T3P23',
                id: 'T3P23',
                xtype: 'hidden'
            }, {
                fieldLabel: '效期',
                name: 'T3P023',
                id: 'T3P023',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true,
                listeners: {
                    change: function (checkbox, newValue, oldValue, eOpts) {
                        if (checkbox.checked) {
                            T3Form.getForm().findField('T3P23').setValue("Y");
                        } else {
                            T3Form.getForm().findField('T3P23').setValue("N");
                        }
                    }
                }
            }
            , {
                fieldLabel: '調撥',
                name: 'T3P24',
                id: 'T3P24',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '繳回',
                name: 'T3P2',
                id: 'T3P25',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '調帳',
                name: 'T3P32',
                id: 'T3P32',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '退貨',
                name: 'T3P26',
                id: 'T3P26',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '報廢',
                name: 'T3P27',
                id: 'T3P27',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '換貨',
                name: 'T3P28',
                id: 'T3P28',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '戰備換貨',
                name: 'T3P29',
                id: 'T3P29',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '院內碼是否作廢',
                name: 'T3P30',
                id: 'T3P30',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '是否寄售',
                name: 'T3P31',
                id: 'T3P31',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }, {
                fieldLabel: '庫房是否作廢',
                name: 'T3P33',
                id: 'T3P33',
                xtype: 'checkboxfield',
                labelWidth: T3FormlabelWidth,
                labelAlign: 'right',
                checked: true
            }

        ]
    });

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 600;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '匯出',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }

    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }
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
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                //height: '40%',
                split: true,
                items: [T1Grid]
            }
        ]
    });

    var d = new Date();
    m = d.getMonth(); //current month
    y = d.getFullYear(); //current year

    T1Query.getForm().findField('P1').setValue(new Date(y, m));

    Ext.getCmp('t1print').setDisabled(true);
    Ext.getCmp('t1export').setDisabled(true);
    T1Query.getForm().findField('P6').setValue('N');
    T1Query.getForm().findField('P8').setValue('N');
    T3Form.getForm().findField('T3P1').setValue('Y');
    T3Form.getForm().findField('T3P2').setValue('Y');
    T3Form.getForm().findField('T3P3').setValue('Y');
    T3Form.getForm().findField('T3P4').setValue('Y');
    T3Form.getForm().findField('T3P5').setValue('Y');
    T3Form.getForm().findField('T3P6').setValue('Y');
    T3Form.getForm().findField('T3P7').setValue('Y');
    T3Form.getForm().findField('T3P8').setValue('Y');
    T3Form.getForm().findField('T3P9').setValue('Y');
    T3Form.getForm().findField('T3P10').setValue('Y');
    T3Form.getForm().findField('T3P11').setValue('Y');
    T3Form.getForm().findField('T3P12').setValue('Y');
    T3Form.getForm().findField('T3P13').setValue('Y');
    T3Form.getForm().findField('T3P14').setValue('Y');
    T3Form.getForm().findField('T3P15').setValue('Y');
    T3Form.getForm().findField('T3P16').setValue('Y');
    T3Form.getForm().findField('T3P17').setValue('Y');
    T3Form.getForm().findField('T3P18').setValue('Y');
    T3Form.getForm().findField('T3P19').setValue('Y');
    T3Form.getForm().findField('T3P20').setValue('Y');
    T3Form.getForm().findField('T3P21').setValue('Y');
    T3Form.getForm().findField('T3P22').setValue('Y');
    T3Form.getForm().findField('T3P23').setValue('Y');
    T3Form.getForm().findField('T3P24').setValue('Y');

});