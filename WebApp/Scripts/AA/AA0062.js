Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = '近效期報表'
    var MATComboGet = '../../../api/AA0062/GetMATCombo';
    var GetWh_no = '../../../api/AA0062/GetWh_no';
    var reportUrl = '/Report/A/AA0062.aspx';
    var T1GetExcel = '../../../api/AA0062/Excel';

    var WHNO_MM1;
    var tempCLS;
    var parCLS = '';
    var printCLS;
    var T1LastRec = null;
    var getRadio;
    var getWhName;
    var getMatName;

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    // 物料類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
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
                    T1Query.getForm().findField("P0").setValue("");
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

    var T1Store = Ext.create('WEBAPP.store.AA.AA0062VM', {
        listeners: {
            beforeload: function (store, options) {
                if (T1Query.getForm().findField('P0').getValue() !== '') {
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').rawValue,
                        p2: T1Query.getForm().findField('P2').rawValue,
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: WHNO_MM1,
                        p5: false
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
                        p2: T1Query.getForm().findField('P2').rawValue,
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: WHNO_MM1,
                        p5: true
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
                            fieldLabel: '臨屆效期區間',
                            name: 'P1',
                            labelWidth: 100,
                            width: 180,
                            padding: '0 4 0 4',
                            fieldCls: 'required',
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);
                                }
                            },
                            editable: false
                        }, {
                            xtype: 'monthfield',
                            fieldLabel: '至',
                            name: 'P2',
                            labelWidth: 20,
                            width: 100,
                            padding: '0 4 0 4',
                            labelSeparator: '',
                            fieldCls: 'required',
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
                            width: 170,
                            padding: '0 4 0 4',
                            //store: Wh_noQueryStore,                            
                            //displayField: 'TEXT',
                            //valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            //fieldCls: 'required',
                            //readOnly: true,
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
                            items: [
                                { boxLabel: '庫備品', name: 'P3', id: 'P3_1', inputValue: '1', width: 70, checked: true },
                                { boxLabel: '非庫備品', name: 'P3', id: 'P3_0', inputValue: '0' }
                            ],
                            width: 150,
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                    Ext.getCmp('t1export').setDisabled(true);
                                }
                            },
                            padding: '0 4 0 4',
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                var MAT_CLASS = T1Query.getForm().findField('P0').getValue();
                                //var tempWH_NO = T1Query.getForm().findField('WH_NO').getValue();
                                var EXP_DATE_B = T1Query.getForm().findField('P1').rawValue;
                                var EXP_DATE_E = T1Query.getForm().findField('P2').rawValue;

                                if (WHNO_MM1 != null && WHNO_MM1 != '' && EXP_DATE_B != null && EXP_DATE_B != '' && EXP_DATE_E != null && EXP_DATE_E != '') {
                                    msglabel('訊息區:');
                                    T1Load();
                                    Ext.getCmp('t1print').setDisabled(false);
                                    Ext.getCmp('t1export').setDisabled(false);
                                }
                                else {
                                    Ext.MessageBox.alert('提示', '請選擇<span style="color:red; font-weight:bold">臨屆效期區間</span>再進行查詢。');
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
                                setWh_no();
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
                    if (T1Store.getCount() > 0) {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '請先建立報表資料。');
                        msglabel('訊息區:請先建立報表資料。');
                    }
                }
            },
            {
                id: 't1export', text: '匯出', disabled: true, handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    var p5 = '';
                    var excelRadio;
                    if (T1Query.getForm().findField('P0').getValue() !== '') {
                        p5 = false;
                        exportCLS = T1Query.getForm().findField('P0').getValue();
                    }
                    else {
                        p5 = true;
                        exportCLS = parCLS;
                    };
                    //取得庫備/非庫備Radio
                    if (T1Query.getForm().findField('P3_1').checked) {
                        excelRadio = '1';
                    }
                    else if (T1Query.getForm().findField('P3_0').checked) {
                        excelRadio = '0';
                    }
                    else if (T1Query.getForm().findField('P3_2').checked) {
                        excelRadio = '2';
                    }
                    p.push({ name: 'FN', value: today + '_近效期報表.xls' });
                    p.push({ name: 'P0', value: exportCLS });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'P2', value: T1Query.getForm().findField('P2').rawValue });
                    p.push({ name: 'P3', value: excelRadio });
                    p.push({ name: 'P4', value: WHNO_MM1 });
                    p.push({ name: 'P5', value: p5 });
                    if (T1Store.getCount() > 0) {
                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    }
                    else {
                        Ext.Msg.alert('訊息', '請先建立報表資料。');
                        msglabel('訊息區:請先建立報表資料。');
                    }
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
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 80
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            },
            //{
            //    text: "品名",
            //    dataIndex: 'MMNAME',
            //    width: 400
            //},
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 300
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 400
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 100
            },
            {
                //xtype: 'datecolumn',
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 80
                //format: 'Xmd'
            },
            {
                text: "數量",
                dataIndex: 'INV_QTY',
                align: 'right', // Right align the contents
                style: 'text-align:left', // Keep left align for Header
                width: 80
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 50
            },
            {
                text: "供貨商",
                dataIndex: 'M_AGENNO',
                width: 230
            },
            {
                text: "合約單價",
                dataIndex: 'M_CONTPRICE',
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
            var p5 = '';            
            if (T1Query.getForm().findField('P0').getValue() !== '') {
                p5 = false;
                printCLS = T1Query.getForm().findField('P0').getValue();
                getMatName = T1Query.getForm().findField('P0').rawValue;
            }
            else {
                p5 = true;
                printCLS = parCLS;
                getMatName = "全部" ;
            };

            //取得庫備/非庫備Radio
            if (T1Query.getForm().findField('P3_1').checked) {
                getRadio = '1';
            }
            else if (T1Query.getForm().findField('P3_0').checked) {
                getRadio = '0';
            }
            else {
                getRadio = '';
            }

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?MAT_CLASS=' + printCLS
                    + '&FRWH=' + WHNO_MM1
                    + '&clsALL=' + p5
                    + '&M_STOREID=' + getRadio
                    + '&EXP_DATE_B=' + T1Query.getForm().findField('P1').rawValue
                    + '&EXP_DATE_E=' + T1Query.getForm().findField('P2').rawValue
                    + '&getWhName=' + getWhName
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

    T1Query.getForm().findField('P1').setValue(new Date(y, m, 1));
    T1Query.getForm().findField('P2').setValue(new Date(y, m + 6, 0));

    Ext.getCmp('t1print').setDisabled(true);
    Ext.getCmp('t1export').setDisabled(true);

});