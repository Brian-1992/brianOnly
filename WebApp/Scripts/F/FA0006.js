Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = '中央庫房調整前庫存成本報表'
    var MATComboGet = '../../../api/FA0006/GetMatCombo';
    var GetWh_no = '../../../api/FA0006/GetWh_no';
    var reportUrl = '/Report/F/FA0006.aspx';
    var T1GetExcel = '../../../api/FA0006/Excel';

    var WHNO_MM1;
    var tempCLS;
    var parCLS = '';
    var printCLS;
    var T1LastRec = null;
    var getRadio;
    //var getWhName;
    //var getMatName;
    var getStoreIDRadio;

    // 物料類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
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

    var getThisMonth = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        //var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        //d = d.length > 1 ? d : "0" + d;
        return y + m;
    }

    var getLastMonth = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth()).toString();
        //var d = (new Date().getDate()).toString();
        if (m == "0") {
            m = "12";
            y = (Number(y) - 1).toString();
        };
        m = m.length > 1 ? m : "0" + m;
        //d = d.length > 1 ? d : "0" + d;
        return y + m;
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

    function setWh_no() {
        Ext.Ajax.request({
            url: GetWh_no,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        T1Query.getForm().findField('WH_NO').setValue(wh_no[0].TEXT);
                        WHNO_MM1 = wh_no[0].VALUE;
                        getWhName = wh_no[0].TEXT;
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWh_no();

    var T1Store = Ext.create('WEBAPP.store.FA0006VM', {
        listeners: {
            beforeload: function (store, options) {
                if (T1Query.getForm().findField('P0').getValue() !== '') {
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').rawValue,
                        //p2: T1Query.getForm().findField('P2').rawValue,
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: T1Query.getForm().findField('P4').getValue(),
                        //p5: T1Query.getForm().findField('P5').getValue(),
                        p6: false
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
                        //p2: T1Query.getForm().findField('P2').rawValue,
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: T1Query.getForm().findField('P4').getValue(),
                        //p5: T1Query.getForm().findField('P5').getValue(),
                        p6: true
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
                            xtype: 'monthfield',
                            fieldLabel: '成本年月',
                            name: 'P1',
                            value: new Date(),
                            labelWidth: 60,
                            width: 140,
                            padding: '0 4 0 4',
                            fieldCls: 'required',
                            allowBlank: false,
                            autoSelect: true,
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
                            editable: false
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '庫房別',
                            name: 'WH_NO',
                            fieldStyle: 'color:blue; font-weight:bold;',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            queryMode: 'local',
                            anyMatch: true,
                            editable: false
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '軍民別',
                            name: 'P4',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
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
                            store: [
                                { VALUE: '0', TEXT: '' },
                                { VALUE: '1', TEXT: '軍用' },
                                { VALUE: '2', TEXT: '民用' }
                            ],
                            value: '0',
                            editable: false
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'P3',
                            padding: '0 4 0 4',
                            items: [
                                { boxLabel: '庫備品', name: 'P3', id: 'P3_0', inputValue: '0', width: 70, checked: true },
                                { boxLabel: '非庫備品', name: 'P3', id: 'P3_1', inputValue: '1', width: 80 },
                                { boxLabel: '庫備品(管控項目)', name: 'P3', id: 'P3_2', inputValue: '2' }
                            ],
                            width: 280,
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
                            handler: function () {
                                var DATA_YM = T1Query.getForm().findField('P1').rawValue;

                                if (DATA_YM != null && DATA_YM != '') {
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
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                Ext.getCmp('t1print').setDisabled(true);
                                Ext.getCmp('t1export').setDisabled(true);
                            }
                        }]
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
                    //    showReport();
                    //else
                    //    Ext.Msg.alert('訊息', '請先建立報表資料。');
                    showReport();
                }
            },
            {
                id: 't1export', text: '匯出', disabled: true, handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    var p6 = '';
                    var excelRadio;
                    if (T1Query.getForm().findField('P0').getValue() !== '') {
                        p6 = false;
                        exportCLS = T1Query.getForm().findField('P0').getValue();
                    }
                    else {
                        p6 = true;
                        exportCLS = parCLS;
                    };

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

                    p.push({ name: 'FN', value: today + '_中央庫房調整前庫存成本報表.xls' });
                    p.push({ name: 'P0', value: exportCLS });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                    //p.push({ name: 'P2', value: T1Query.getForm().findField('P2').rawValue });
                    p.push({ name: 'P3', value: excelRadio });
                    p.push({ name: 'P4', value: T1Query.getForm().findField('P4').getValue() });
                    //p.push({ name: 'P5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'P6', value: p6 });
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
                width: 70
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
                width: 400
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                text: "上期結存單價(軍)",
                dataIndex: 'PMN_AVGPRICEA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "上期結存單價(民)",
                dataIndex: 'PMN_AVGPRICEB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "戰備存量",
                dataIndex: 'PINV_QTYA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "民品存量",
                dataIndex: 'PINV_QTYB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "合計存量",
                dataIndex: 'PINVQTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期初戰備成本",
                dataIndex: 'SUM1A',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期初民品成本",
                dataIndex: 'SUM1B',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期初成本",
                dataIndex: 'SUM1',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "進貨單價(軍)",
                dataIndex: 'IN_PRICEA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "進貨單價(民)",
                dataIndex: 'IN_PRICEB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "戰備進貨量",
                dataIndex: 'IN_QTYA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "民品進貨量",
                dataIndex: 'IN_QTYB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "進貨總量",
                dataIndex: 'INQTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "戰備進貨成本",
                dataIndex: 'SUM2A',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "民品進貨成本",
                dataIndex: 'SUM2B',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "進貨成本",
                dataIndex: 'SUM2',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "消耗單價(軍)",
                dataIndex: 'AVG_PRICEA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "消耗單價(民)",
                dataIndex: 'AVG_PRICEB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "戰備消耗量",
                dataIndex: 'OUT_QTYA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "民品消耗量",
                dataIndex: 'OUT_QTYB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "消耗總量",
                dataIndex: 'OUTQTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "戰備消耗成本",
                dataIndex: 'SUM3A',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "民品消耗成本",
                dataIndex: 'SUM3B',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "消耗成本",
                dataIndex: 'SUM3',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期末單價(軍)",
                dataIndex: 'AVG_PRICEA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期末單價(民)",
                dataIndex: 'AVG_PRICEB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "戰備存量",
                dataIndex: 'INV_QTYA',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "民品存量",
                dataIndex: 'INV_QTYB',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "合計期末存量",
                dataIndex: 'INVQTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期末戰備成本",
                dataIndex: 'SUM4A',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期末民品成本",
                dataIndex: 'SUM4B',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
            },
            {
                text: "期末成本",
                dataIndex: 'SUM4',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 130
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
            var p6 = '';
            if (T1Query.getForm().findField('P0').getValue() !== '') {
                p6 = false;
                printCLS = T1Query.getForm().findField('P0').getValue();
                getMatName = T1Query.getForm().findField('P0').rawValue.substr(3);
            }
            else {
                p6 = true;
                printCLS = parCLS;
                getMatName = "全部";
            };

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
                + '?MAT_CLASS=' + printCLS
                + '&clsALL=' + p6
                + '&M_STOREID=' + getStoreIDRadio
                + '&DATA_YM=' + T1Query.getForm().findField('P1').rawValue
                //+ '&DATA_YM_E=' + T1Query.getForm().findField('P2').rawValue
                //+ '&MMCODE=' + T1Query.getForm().findField('P5').getValue()
                + '&MIL=' + T1Query.getForm().findField('P4').getValue()
                + '&getM_StoreIDName=' + P3RadioName
                + '&getMatName=' + getMatName
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

    //T1Query.getForm().findField('P1').setValue(new Date(y, m));
    //T1Query.getForm().findField('P2').setValue(new Date(y, m - 1));

    Ext.getCmp('t1print').setDisabled(true);
    Ext.getCmp('t1export').setDisabled(true);
});