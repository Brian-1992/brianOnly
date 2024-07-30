Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    //var T1Set = '';
    var T1Name = '全院及衛星庫房成本報表'
    var MATComboGet = '../../../api/AA0106/GetMatCombo';
    var YMComboGet = '../../../api/AA0106/GetYMCombo';
    //var GetWh_no = '../../../api/AA0106/GetWh_no';
    var reportUrl = '/Report/A/AA0106.aspx';
    var T1GetExcel = '../../../api/AA0106/Excel';

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


    var T1Store = Ext.create('WEBAPP.store.AA.AA0106VM', {
        listeners: {
            beforeload: function (store, options) {
                if (T1Query.getForm().findField('P0').getValue() !== '') {
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').rawValue,
                        p2: T1Query.getForm().findField('P2').getValue(),
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: false
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
                        p2: T1Query.getForm().findField('P2').getValue(),
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: true
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
                            width: 170,
                            padding: '4 4 4 4',
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
                            fieldLabel: '單位類別',
                            name: 'P2',
                            padding: '4 4 4 4',
                            width: 480,
                            items: [
                                {
                                    checked: true,
                                    width: 100,
                                    boxLabel: '應盤點單位',
                                    name: 'P2',
                                    id: 'P2_0',
                                    inputValue: '0'
                                }, {
                                    width: 100,
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
                                    width: 100,
                                    boxLabel: '全院',
                                    name: 'P2',
                                    id: 'P2_3',
                                    inputValue: '3'
                                }],
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);
                                }
                            }
                        }]
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
                    width: 170,
                    padding: '4 4 4 4',
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
                    editable: false
                },
                {
                    xtype: 'radiogroup',
                    name: 'P3',
                    padding: '4 4 4 4',
                    width: 360,
                    fieldLabel: '是否庫備',
                    items: [
                        {
                            boxLabel: '不區分',
                            name: 'P3',
                            id: 'P3_0',
                            inputValue: '0',
                            width: 100,
                            checked: true
                        },
                        {
                            boxLabel: '庫備品',
                            name: 'P3',
                            id: 'P3_1',
                            inputValue: '1',
                            width: 100
                        },
                        {
                            boxLabel: '非庫備品',
                            name: 'P3', id: 'P3_2',
                            inputValue: '2',
                            width: 100
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
                    xtype: 'button',
                    text: '查詢',
                    margin: '4 0 4 0',
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
                    //if (T1Store.getCount() > 0)
                    showReport();
                    //else
                    //    Ext.Msg.alert('訊息', '請先建立報表資料。');
                }
            },
            {
                id: 't1export', text: '匯出', disabled: true, handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    var p4 = '';
                    var excelP2Radio;
                    var excelP3Radio;
                    if (T1Query.getForm().findField('P0').getValue() !== '') {
                        p4 = false;
                        exportCLS = T1Query.getForm().findField('P0').getValue();
                    }
                    else {
                        p4 = true;
                        exportCLS = parCLS;
                    };
                    //取得單位類別Radio
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

                    p.push({ name: 'FN', value: today + '_全院及衛星庫房成本報表.xls' });
                    p.push({ name: 'P0', value: exportCLS });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'P2', value: excelP2Radio });
                    p.push({ name: 'P3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'P4', value: p4 });
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
                //xtype: 'textfield',
                text: "物料分類",
                width: 70,
                renderer: function () {
                    var rtn = '';
                    if (T1Query.getForm().findField('P0').getValue() == "") {
                        rtn = '全部類';
                    }
                    else {
                        rtn = T1Query.getForm().findField('P0').getRawValue();
                    }
                    return rtn;
                }
            },
            {
                text: "單位代碼",
                dataIndex: 'WH_NO',
                width: 70
            },
            {
                text: "單位名稱",
                dataIndex: 'WH_NAME',
                width: 300
            },
            {
                text: "上月結存金額",
                dataIndex: 'SUM1',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月進貨金額",
                dataIndex: 'SUM2',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月消耗金額",
                dataIndex: 'SUM3',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "本月結存金額",
                dataIndex: 'SUMTOT',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
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

            //取得單位類別的Radio
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

            //取得是否庫備Radio
            if (T1Query.getForm().findField('P3_0').checked) {
                P3RadioName = T1Query.getForm().findField('P3_0').boxLabel;
            }
            else if (T1Query.getForm().findField('P3_1').checked) {
                P3RadioName = T1Query.getForm().findField('P3_1').boxLabel;
            }
            else if (T1Query.getForm().findField('P3_2').checked) {
                P3RadioName = T1Query.getForm().findField('P3_2').boxLabel;
            }

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?MAT_CLASS=' + printCLS
                    + '&SET_YM=' + T1Query.getForm().findField('P1').rawValue
                    + '&INIDFLAG=' + getInidFlagRadio
                    + '&M_STOREID=' + T1Query.getForm().findField('P3').getValue()
                    + '&clsALL=' + p4
                    + '&getMatName=' + getMatName
                    + '&getInidFlagName=' + P2RadioName
                    + '&getM_StoreIDName=' + P3RadioName
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