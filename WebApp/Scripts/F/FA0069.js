Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = '成本單位明細報表'
    var MATComboGet = '../../../api/FA0069/GetMatCombo';
    var YMComboGet = '../../../api/FA0069/GetYMCombo';
    var reportUrl = '/Report/F/FA0069.aspx';
    var T1GetExcel = '../../../api/FA0069/Excel';

    //var WHNO_MM1;
    var tempCLS;
    var parCLS = '';
    var printCLS;
    var T1LastRec = null;
    var getStoreIDRadio;
    var getInidFlagRadio;
    var tmpYM;
    //var getWhName;
    var getMatName;

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
        //fieldCls: 'required',
        //allowBlank: false,
        padding: '4 4 4 4',
        width: 150,
        disabled: true,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0069/GetWH_NoCombo',

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
            url: '/api/FA0023/GetYNCombo',
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

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },           //單位代碼,
            { name: 'WH_NAME', type: 'string' },         //單位名稱,
            { name: 'DATA_YM', type: 'string' },         //成本年月,
            { name: 'MMCODE', type: 'string' },          //院內碼,
            { name: 'MMNAME_C', type: 'string' },            //中文品名,
            { name: 'MMNAME_E', type: 'string' },            //英文品名,
            { name: 'BASE_UNIT', type: 'string' },           //計量單位,
            { name: 'PMN_AVGPRICE', type: 'string' },            //期初單價,
            { name: 'P_INV_QTY', type: 'string' },           //期初結存,
            { name: 'SUM1', type: 'string' },            //期初金額,
            { name: 'IN_PRICE', type: 'string' },            //進貨單價,
            { name: 'IN_QTY', type: 'string' },          //本月進貨,
            { name: 'SUM2', type: 'string' },            //進貨金額,
            { name: 'DISC_CPRICE', type: 'string' },           //消耗單價,本月單價,
            { name: 'OUT_QTY', type: 'string' },         //本月消耗,
            { name: 'SUM3', type: 'string' },            //消耗金額,
            { name: 'INV_QTY', type: 'string' },         //本月結存,
            { name: 'SUMTOT', type: 'string' },          //結存金額,
            { name: 'INVENTORYQTY', type: 'string' },            //盤盈虧數量,
            { name: 'SUM4', type: 'string' },			//盤盈虧金額,
            { name: 'WH_TURNOVER', type: 'string' },			//庫房周轉率
            { name:'SUM5', type:'string'}               //差異金額
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            timeout: 180000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0069/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
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
        matchFieldWidth: false,
        listConfig: { width: 300 },
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T1Query.getForm();
            return {
                MMCODE: f.findField('P7').getValue(),
                MAT_CLASS: '02',
                clsALL: false
            };
        },

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/FA0069/GetMMCodeCombo',

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
                            width: 534,
                            items: [
                                {
                                    checked: true,
                                    width: 90,
                                    boxLabel: '應盤點單位',
                                    name: 'P2',
                                    id: 'P2_0',
                                    inputValue: '0'
                                }, {
                                    width: 90,
                                    boxLabel: '行政科室',
                                    name: 'P2',
                                    id: 'P2_1',
                                    inputValue: '1'
                                }, {
                                    width: 100,
                                    boxLabel: '財務獨立單位',
                                    name: 'P2',
                                    id: 'P2_2',
                                    inputValue: '2'
                                }, {
                                    width: 60,
                                    boxLabel: '全院',
                                    name: 'P2',
                                    id: 'P2_3',
                                    inputValue: '3',
                                    listeners: {
                                        change: function (radioGroup, newValue, oldValue) {
                                            Ext.getCmp('t1print').setDisabled(true);
                                            Ext.getCmp('t1export').setDisabled(true);

                                        }
                                    }
                                }, {
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
                                                T1Query.getForm().findField('P5').setDisabled(true);
                                                T1Query.getForm().findField('P3_3').setDisabled(true);

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
                    xtype: 'displayfield',
                    fieldLabel: '物料分類',
                    value: '02 衛材',
                    labelWidth: 60,
                    width: 130,
                    padding: '4 4 4 4',
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
                            disabled: true
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

                        Ext.getCmp('t1print').setDisabled(true);
                        Ext.getCmp('t1export').setDisabled(true);
                    }
                }
            ]
        }, {
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
                    editable: false
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
                    var today = getTodayDate();
                    var p = new Array();
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
                    p.push({ name: 'P0', value: '02,' });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'P2', value: excelP2Radio });
                    p.push({ name: 'P3', value: excelP3Radio });
                    p.push({ name: 'P4', value: true });
                    p.push({ name: 'P5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'P6', value: T1Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'P7', value: T1Query.getForm().findField('P7').getValue() });
                    p.push({ name: 'P8', value: T1Query.getForm().findField('P8').getValue() });
                    p.push({ name: 'P9', value: T1Query.getForm().findField('P9').getValue() });
                    p.push({ name: 'P10', value: T1Query.getForm().findField('P10').getValue() });
                    p.push({ name: 'P11', value: T1Query.getForm().findField('P11').getValue() });
                    p.push({ name: 'P12', value: T1Query.getForm().findField('P12').getValue() });

                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
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
                text: "上月結存金額",
                dataIndex: 'SUM1',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130,
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            },
            {
                text: "本月進貨金額",
                dataIndex: 'SUM2',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130,
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            },
            {
                text: "本月消耗金額",
                dataIndex: 'SUM3',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130,
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            },
            {
                text: "盤盈虧金額",
                dataIndex: 'SUM4',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月結存金額",
                dataIndex: 'SUMTOT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130,
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            },
            {
                text: "差異金額",
                dataIndex: 'SUM5',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130,
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            },
            {
                text: "周轉率",
                dataIndex: 'WH_TURNOVER',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130,
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
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
                    + '?MAT_CLASS=02'
                    + '&SET_YM=' + T1Query.getForm().findField('P1').rawValue
                    + '&INIDFLAG=' + getInidFlagRadio
                    + '&M_STOREID=' + getStoreIDRadio
                    + '&clsALL=' + false
                    + '&WH_NO=' + T1Query.getForm().findField('P5').rawValue
                    + '&getMatName=' + getMatName
                    + '&getInidFlagName=' + P2RadioName
                + '&IS_INV_MINUS=' + T1Query.getForm().findField('P6').getValue()
                + '&MMCODE=' + T1Query.getForm().findField('P7').getValue()
                + '&IS_OUT_MINUS=' + T1Query.getForm().findField('P8').getValue()
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


});