Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = '庫存成本報表'
    var WHComboGet = '../../../api/AB0120/GetWHCombo';
    var MATComboGet = '../../../api/AB0120/GetMatCombo';
    var YMComboGet = '../../../api/AB0120/GetYMCombo';
    var GetWh_no = '../../../api/AB0120/getWH_NO';
    var reportUrl = '/Report/A/AB0120.aspx';
    var T1GetExcel = '../../../api/AB0120/Excel';

    var WHNO_MM1;
    var tempCLS;
    var parCLS = '';
    var printCLS;
    var T1LastRec = null;
    var getStoreIDRadio;
    var tmpYM;
    var tmpWH;
    var getMatName;

    // 庫房代碼清單
    var WHQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 物料類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    // 成本年月清單
    var YMQueryStore = Ext.create('Ext.data.Store', {
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

    function setWHComboData() {
        Ext.Ajax.request({
            url: WHComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var set_wh = data.etts;
                    if (set_wh.length > 0) {
                        for (var i = 0; i < set_wh.length; i++) {
                            WHQueryStore.add({ VALUE: set_wh[i].VALUE, TEXT: set_wh[i].TEXT });
                            if (i == 0) {
                                tmpWH = set_wh[i];
                            }
                        }
                    }
                    T1Query.getForm().findField('WH_NO').setValue("");  //預設第一筆
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWHComboData();

    function setMATComboData(wh_mat) {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            params: {
                wh_no: wh_mat
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    MATQueryStore.removeAll();
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

    var T1Store = Ext.create('WEBAPP.store.AB.AB0120VM', {
        listeners: {
            beforeload: function (store, options) {
                if (T1Query.getForm().findField('P0').getValue() !== '') {
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').rawValue,
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: false,
                        p5: T1Query.getForm().findField('WH_NO').getValue(),
                        p6: T1Query.getForm().findField('P6').checked ? 'true' : ''
                    };                     
                }
                else {
                    parCLS = "";
                    for (var i = 0; i < tempCLS.length; i++) {
                        parCLS += tempCLS[i].VALUE + ',';
                    };
                    var np = {
                        p0: parCLS,
                        p1: T1Query.getForm().findField('P1').rawValue,
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: true,
                        p5: T1Query.getForm().findField('WH_NO').getValue(),
                        p6: T1Query.getForm().findField('P6').checked ? 'true' : ''
                    };                    
                };
                Ext.apply(store.proxy.extraParams, np);
            }
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
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                            width: 170,
                            padding: '0 4 0 4',
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
                            xtype: 'combo',
                            fieldLabel: '庫房別',
                            name: 'WH_NO',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 240,
                            padding: '0 4 0 4',
                            store: WHQueryStore,
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
                                    T1Query.getForm().findField('P0').setDisabled(false);
                                    T1Query.getForm().findField('P0').setValue("");
                                    if (T1Query.getForm().findField('WH_NO').getValue() !== null || T1Query.getForm().findField('WH_NO').getValue() !== "") {
                                        setMATComboData(T1Query.getForm().findField('WH_NO').getValue());
                                    }
                                }
                            },
                            editable: true
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '物料分類',
                            name: 'P0',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4',
                            store: MATQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            //fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);
                                }
                            },
                            editable: false,
                            disabled: true
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'P3',
                            padding: '0 4 0 4',
                            width: 330,
                            fieldLabel: '是否庫備',
                            items: [
                                {
                                    boxLabel: '不區分',
                                    name: 'P3',
                                    id: 'P3_0',
                                    inputValue: '0',
                                    width: 60,
                                    checked: true
                                },
                                {
                                    boxLabel: '庫備品',
                                    name: 'P3',
                                    id: 'P3_1',
                                    inputValue: '1',
                                    width: 60
                                },
                                {
                                    boxLabel: '非庫備品',
                                    name: 'P3', id: 'P3_2',
                                    inputValue: '2',
                                    width: 70
                                },
                                
                            ],
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);
                                }
                            }
                        },
                        {
                            xtype: 'fieldcontainer',
                            fieldLabel: '僅顯示數量不為0之品項',
                            defaultType: 'checkboxfield',
                            labelWidth: 160,
                            labelSeparator: '',
                            width: 180,
                            items: [
                                {
                                    name: 'P6',
                                    inputValue: '1',
                                }
                            ]
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            margin: '0 4 0 4',
                            handler: function () {
                                var SET_YM = T1Query.getForm().findField('P1').rawValue;

                                if (!T1Query.getForm().findField('P1').rawValue ||
                                    !T1Query.getForm().findField('WH_NO').rawValue) {
                                    Ext.MessageBox.alert('提示', '<span style="color:red; font-weight:bold">成本年月</span>與庫房代碼為必填');
                                    return; 
                                }

                                if (SET_YM != null && SET_YM != '') {
                                    msglabel('訊息區:');
                                    T1Load();
                                    Ext.getCmp('t1print').setDisabled(false);
                                    Ext.getCmp('t1export').setDisabled(false);
                                }
                                else {
                                   
                                }
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            margin: '0 4 0 4',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                Ext.getCmp('t1print').setDisabled(true);
                                Ext.getCmp('t1export').setDisabled(true);
                            }
                        }
                    ]
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
                id: 't1print', text: '列印', disabled: true, handler: function () {
                    showReport();
                }
            },
            {
                id: 't1export', text: '匯出', disabled: true, handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    var p4 = '';
                    var excelP3Radio;
                    if (T1Query.getForm().findField('P0').getValue() !== '') {
                        p4 = false;
                        exportCLS = T1Query.getForm().findField('P0').getValue();
                    }
                    else {
                        p4 = true;
                        exportCLS = parCLS;
                    };

                    p.push({ name: 'FN', value: today + '_' + T1Query.getForm().findField('WH_NO').getValue() +'_庫存成本報表.xlsx' });
                    p.push({ name: 'P0', value: exportCLS });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'P3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'P4', value: p4 });
                    p.push({ name: 'P5', value: T1Query.getForm().findField('WH_NO').getValue() });
                    p.push({ name: 'P6', value: T1Query.getForm().findField('P6').checked ? 'true' : ''})
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 350
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 350
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "上月結存",
                dataIndex: 'P_INV_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "上期結存單價",
                dataIndex: 'PMN_AVGPRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 90
            },
            {
                text: "上月結存金額",
                dataIndex: 'SUM1',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月進貨",
                dataIndex: 'IN_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "進貨單價",
                dataIndex: 'IN_PRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "本月進貨金額",
                dataIndex: 'SUM2',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月消耗",
                dataIndex: 'OUT_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "消耗單價",
                dataIndex: 'DISC_CPRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "本月消耗金額",
                dataIndex: 'SUM3',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "差異金額",
                dataIndex: 'D_AMT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "本期結存",
                dataIndex: 'TOT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "期末單價",
                dataIndex: 'DISC_CPRICE',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "本月結存金額",
                dataIndex: 'SUMTOT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月盤盈虧",
                dataIndex: 'INVENTORYQTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 90
            },
            {
                text: "本月盤盈虧金額",
                dataIndex: 'SUM4',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期末比值",
                dataIndex: 'RAT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "周轉率",
                dataIndex: 'TURNOVER',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
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

            //取得是否庫備Radio
            if (T1Query.getForm().findField('P3').getValue() == '0') {
                P3RadioName = T1Query.getForm().findField('P3_0').boxLabel;
            } else if (T1Query.getForm().findField('P3').getValue() == '1'){
                P3RadioName = T1Query.getForm().findField('P3_1').boxLabel;
            } else {
                P3RadioName = T1Query.getForm().findField('P3_2').boxLabel;
            }

            p5 = T1Query.getForm().findField('WH_NO').getValue();

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?MAT_CLASS=' + printCLS
                    + '&SET_YM=' + T1Query.getForm().findField('P1').rawValue
                + '&M_STOREID=' + T1Query.getForm().findField('P3').getValue()
                    + '&clsALL=' + p4
                    + '&WH_NO=' + p5
                    + '&MAT_CLSNAME=' + getMatName
                    + '&M_STOREIDNAME=' + P3RadioName
                    + '&NotZeroOnly=' + (T1Query.getForm().findField('P6').checked ? 'true' : '')
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