Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Name = '申請核撥查詢';
    var T1GetExcel = '/api/AA0184/GetExcel';
    var MmcodeComboGet = '/api/AA0184/GetMMCodeCombo';
    var defaultClass = "";
    var defaultStartDate = "";
    var y = new Date().getFullYear();
    var m = new Date().getMonth();
    var d = new Date().getDate();
    var defaultEndDate = new Date(y, m, d);
    
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    //物料分類
    var SetMatClassSubComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0184/GetMatClassSubCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var DataCount = store.getCount();
                if (DataCount > 0) {
                    if (menuLink == "FA0073" || menuLink == "FA0091" || menuLink == "FA0075") {
                        defaultClass = "all02";
                        T1Query.getForm().findField('P0').setValue(defaultClass);
                    }
                }
            }
        },
        autoLoad: true
    });
    //申請/核撥日期(起)
    function SetStartApptimeGet() {
        Ext.Ajax.request({
            url: '/api/AA0184/GetStartApptime',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (menuLink == "FA0073" || menuLink == "FA0091") {
                        defaultStartDate = data.etts[0];
                        T1Query.getForm().findField('P1').setValue(defaultStartDate);
                        T1Query.getForm().findField('P2').setValue(defaultEndDate);
                        T1Query.getForm().findField('P4').setValue(defaultStartDate);
                        T1Query.getForm().findField('P5').setValue(defaultEndDate);
                    }
                    else if (menuLink == "FA0075") {
                        defaultStartDate = defaultEndDate;
                        T1Query.getForm().findField('P1').setValue(defaultStartDate);
                        T1Query.getForm().findField('P2').setValue(defaultEndDate);
                        T1Query.getForm().findField('P4').setValue(defaultStartDate);
                        T1Query.getForm().findField('P5').setValue(defaultEndDate);
                    }
                    else {
                        defaultEndDate = data.etts[0];
                        T1Query.getForm().findField('P2').setValue(defaultEndDate);
                        T1Query.getForm().findField('P5').setValue(defaultEndDate);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    SetStartApptimeGet();
    //申請單狀態
    var SetFlowIdComboGet = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0184/GetFlowIdCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P7',
        name: 'P7',
        fieldLabel: '院內碼',
        emptyText: '全部',
        width: 200,
        limit: 10,
        queryUrl: MmcodeComboGet,
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        matchFieldWidth: false,
        listConfig: { width: 300 },
        getDefaultParams: function () {

        },
        listeners: {
        }
    });
    //入庫庫房
    var SetWhNoComboStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function SetWhNoComboGet(MatClassSub) {
        Ext.Ajax.request({
            url: '/api/AA0184/GetWhNoCombo',
            method: reqVal_p,
            params: {
                p0: MatClassSub
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    SetWhNoComboStore.removeAll();
                    var WhNo = data.etts;
                    if (WhNo.length > 0) {
                        for (var i = 0; i < WhNo.length; i++) {
                            SetWhNoComboStore.add({ VALUE: WhNo[i].VALUE, TEXT: WhNo[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    SetWhNoComboGet('');

    var mLabelWidth = 80;
    var mWidth = 200;
    //上方查詢區塊
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '0 4 0 4'
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'container',
                layout: 'hbox',
                padding: '0 0 4 0',
                items: [{
                    xtype: 'combo',
                    store: SetMatClassSubComboGet,
                    name: 'P0',
                    id: 'P0',
                    fieldLabel: '藥材分類',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    requiredFields: ['VALUE'],
                    anyMatch: true,
                    allowBlank: false,
                    fieldCls: 'required',
                    listeners: {
                        select: function (combo, records, eOpts) {
                            SetWhNoComboGet(T1Query.getForm().findField('P0').getValue());
                        }
                    }
                }, {
                    xtype: 'datefield',
                    fieldLabel: '申請日期',
                    name: 'P1',
                    id: 'P1',
                    vtype: 'dateRange',
                    dateRange: { end: 'P2' },
                    regexText: '請選擇日期'
                }, {
                    xtype: 'datefield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '至',
                    labelWidth: 10,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P1' }
                }, {
                    xtype: 'combo',
                    store: SetFlowIdComboGet,
                    name: 'P3',
                    id: 'P3',
                    width: 400,
                    fieldLabel: '申請單狀態',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true
                }, {
                    xtype: 'checkbox',
                    name: 'P8',
                    width: 130,
                    boxLabel: '差異過濾',
                    inputValue: 'Y',
                    checked: false,
                    padding: '0 4 0 8',
                    hidden: (menuLink != "FA0090"),
                    listeners:
                    {
                        change: function (rg, nVal, oVal, eOpts) {
                            setVisibleColumns(nVal);
                        }
                    }
                }]
            }, {
                xtype: 'container',
                layout: 'hbox',
                items: [{
                    xtype: 'datefield',
                    fieldLabel: '核撥日期',
                    name: 'P4',
                    id: 'P4',
                    vtype: 'dateRange',
                    dateRange: { end: 'P5' },
                    regexText: '請選擇日期'
                }, {
                    xtype: 'datefield',
                    id: 'P5',
                    name: 'P5',
                    fieldLabel: '至',
                    labelWidth: 10,
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P4' }
                }, {
                    xtype: 'combo',
                    store: SetWhNoComboStore,
                    name: 'P6',
                    id: 'P6',
                    width: 400, // 小於 400 字會跑版 0206t1
                    fieldLabel: '入庫庫房',
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE'
                },
                    T1QuerymmCodeCombo,
                {
                    xtype: 'button',
                    margin: '0 0 0 20',
                    text: '查詢',
                    handler: function () {
                        msglabel('訊息區:');
                        T1Tool.down('#export').setDisabled(true);
                        T1Load();
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T1Query.getForm().findField('P0').setValue(defaultClass);
                        SetStartApptimeGet();
                    }
                }]
            }]
        }]
    });
    var T1Store = Ext.create('WEBAPP.store.AA.AA0184', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').rawValue,
                    p5: T1Query.getForm().findField('P5').rawValue,
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                        T1Tool.down('#export').setDisabled(false);
                    }
                    else {
                        msglabel('查無資料!');
                        T1Tool.down('#export').setDisabled(true);
                    }
                }
            }
        }
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: true, hidden: (menuLink == "FA0091" || menuLink == "FA0075"),
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件 
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').rawValue }); //SQL篩選條件 
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件 
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').rawValue }); //SQL篩選條件 
                            p.push({ name: 'p5', value: T1Query.getForm().findField('P5').rawValue }); //SQL篩選條件
                            p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() }); //SQL篩選條件
                            p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() }); //SQL篩選條件
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }

        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
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
        columns: [
            { xtype: 'rownumberer', width: 40 }, { text: "申請單號", dataIndex: 'DOCNO', width: 180 }
            , { text: "物料分類", dataIndex: 'MAT_CLASS', width: 80 }
            , { text: "狀態", dataIndex: 'COMBITEM', width: 120 }
            , { text: "出庫庫房", dataIndex: 'FRWH', width: 150 }
            , { text: "入庫庫房", dataIndex: 'TOWH', width: 150 }
            , { text: "申請時間", dataIndex: 'APPTIME', width: 80 }
            , { text: "院內碼", dataIndex: 'MMCODE', width: 80 }
            , { text: "中文名稱", dataIndex: 'MMNAME_C', width: 200 }
            , { text: "英文名稱", dataIndex: 'MMNAME_E', width: 200 }
            , { text: "單位", dataIndex: 'BASE_UNIT', width: 80 }
            , { text: "申請數量", dataIndex: 'APPQTY', width: 80 }
            , { text: "核可數量", dataIndex: 'APVQTY', width: 80 }
            , { text: "核撥日期", dataIndex: 'DIS_TIME', width: 80 }
            , {
                text: "差異",
                align: 'left',
                showOption: false,
                hidden: true,
                renderer: function (val, meta, record) {
                    return Number(record.data.APPQTY) - Number(record.data.APVQTY);
                }
            },
            { header: "", flex: 1 }
        ]
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
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
                itemId: 't1top',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            }
        ]
    });

    // 設定欄位是否顯示
    var setVisibleColumns = function (optVal) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        T1Grid.suspendLayouts();
        for (var i = 1; i < T1Grid.columns.length; i++) {
            if (T1Grid.columns[i].showOption == optVal)
                T1Grid.columns[i].setVisible(false);
            else
                T1Grid.columns[i].setVisible(true);
        }
        T1Grid.resumeLayouts(true);
        myMask.hide();
    };

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
});