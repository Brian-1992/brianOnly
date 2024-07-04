Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});


Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";
    var T1GetExcel = '/api/GA0005/Excel';
    var NAMEComboGet = '../../../api/GA0005/GetNAMECombo';

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;

    var T1tmpTC_TYPE;
    var T2tmpTC_TYPE;

    // 物品類別清單
    var NaQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: NAMEComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            NaQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var cbPurchst = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'T1P3',
        fieldLabel: '訂單狀態',
        queryParam: {
            GRP_CODE: 'TC_PURCH_M',
            DATA_NAME: 'PURCH_ST'
        },
        padding: '0 4 0 4',
        labelWidth: false,
        labelStyle: 'width: 35%',
        labelAlign: 'right',
        width: '25%',
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: false,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbPurchst2 = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'T2P5',
        fieldLabel: '訂單狀態',
        queryParam: {
            GRP_CODE: 'TC_PURCH_M',
            DATA_NAME: 'PURCH_ST'
        },
        padding: '0 4 0 4',
        labelWidth: false,
        labelStyle: 'width: 35%',
        labelAlign: 'right',
        width: '25%',
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: false,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        padding: '4 0 4 0',
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            padding: '0 0 0 2',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '訂單編號',
                    name: 'T1P0',
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    enforceMaxLength: true,
                    maxLength: 13,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '訂購日期',
                    name: 'T1P1',
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    padding: '0 4 0 4',
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'T1P2',
                    labelWidth: 10,
                    width: '20%',
                    padding: '0 4 0 4',
                    labelSeparator: ''
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            padding: '2 0 0 0',
            items: [
                cbPurchst,
                {
                    xtype: 'radiogroup',
                    fieldLabel: '藥品種類',
                    name: 'T1P4',
                    anchor: '35%',
                    width: '45%',
                    labelAlign: 'right',
                    items: [
                        { boxLabel: '科學中藥', width: '50%', name: 'TC_TYPE', inputValue: 1, checked: true },
                        { boxLabel: '飲片', width: '50%', name: 'TC_TYPE', inputValue: 2 }
                    ]
                }, 
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1LastRec = null;
                        T11Store.removeAll();
                        T1Tool.down('#export').setDisabled(true);
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('T1P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }


            ]
        }]
    });
    var T11Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        padding: '4 0 4 0',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP11',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '藥品名稱',
                    name: 'T11P0',
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    enforceMaxLength: true,
                    maxLength: 250,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '藥商名稱',
                    name: 'T11P1',
                    enforceMaxLength: true,
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    padding: '0 4 0 4',
                    store: NaQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    itemId: 'btnT11Query',
                    text: '查詢',
                    handler: function () {
                        T11Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('T11P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }


            ]
        }]
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'panel',
            id: 'PanelP20',
            border: false,
            layout: 'hbox',
            padding: '0 0 0 2',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '訂單編號',
                    name: 'T2P0',
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    enforceMaxLength: true,
                    maxLength: 13,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '訂購日期',
                    name: 'T2P1',
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    padding: '0 4 0 4',
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'T2P2',
                    labelWidth: 10,
                    width: '20%',
                    padding: '0 4 0 4',
                    labelSeparator: ''
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP21',
            border: false,
            layout: 'hbox',
            padding: '2 0 0 0',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '藥品名稱',
                    name: 'T2P3',
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    enforceMaxLength: true,
                    maxLength: 250,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combo',
                    fieldLabel: '藥商名稱',
                    name: 'T2P4',
                    enforceMaxLength: true,
                    labelWidth: false,
                    labelStyle: 'width: 35%',
                    labelAlign: 'right',
                    width: '25%',
                    padding: '0 4 0 4',
                    store: NaQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP22',
            border: false,
            layout: 'hbox',
            padding: '2 0 0 0',
            items: [
                cbPurchst2,
                {
                    xtype: 'radiogroup',
                    fieldLabel: '藥品種類',
                    name: 'T2P6',
                    anchor: '35%',
                    width: '45%',
                    labelAlign: 'right',
                    items: [
                        { boxLabel: '科學中藥', width: '50%', name: 'TC_TYPE', inputValue: 1, checked: true },
                        { boxLabel: '飲片', width: '50%', name: 'TC_TYPE', inputValue: 2 }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T2Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('T2P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }


            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['PUR_NO', 'PUR_DATE', 'TC_TYPE', 'PUR_UNM', 'PUR_NOTE,', 'PURCH_ST']
    });
    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['MMCODE', 'MMNAME_C', 'AGEN_NAMEC', 'PUR_QTY,', 'PUR_UNIT', 'IN_PURPRICE', 'PUR_AMOUNT']
    });
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['PUR_NO', 'PUR_DATE', 'TC_TYPE', 'PUR_UNM', 'PUR_NOTE,', 'PURCH_ST', 'MMCODE', 'MMNAME_C', 'AGEN_NAMEC', 'PUR_QTY', 'PUR_UNIT', 'IN_PURPRICE', 'PUR_AMOUNT']
    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'PUR_DATE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var f = T1Query.getForm();
                // 載入前將查詢條件代入參數
                var np = {
                    P0: f.findField('T1P0').getValue(),
                    P1: f.findField('T1P1').getValue(),
                    P2: f.findField('T1P2').getValue(),
                    P3: f.findField('T1P3').getValue(),
                    P4: f.findField('T1P4').getValue()['TC_TYPE']
                };
                Ext.apply(store.proxy.extraParams, np);

            },
        },
        proxy: {
            type: 'ajax',
            //timeout: 90000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/GA0005/MasterAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });
    var T11Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T11Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var f = T11Query.getForm();
                // 載入前將查詢條件代入參數
                var np = {
                    P0: T1LastRec.data['PUR_NO'],
                    P1: f.findField('T11P0').getValue(),
                    P2: f.findField('T11P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);

            },
            load: function (store, records, successful, eOpts) {
                if (store.data.items.length > 0)
                    T1Tool.down('#export').setDisabled(false);
                else
                    T1Tool.down('#export').setDisabled(true);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/GA0005/DetailAll',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    var T2Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T2Model',
        pageSize: 20,
        remoteSort: true,

        sorters: [{ property: 'PUR_DATE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var f = T2Query.getForm();
                // 載入前將查詢條件代入參數
                var np = {
                    P0: f.findField('T2P0').getValue(),
                    P1: f.findField('T2P1').getValue(),
                    P2: f.findField('T2P2').getValue(),
                    P3: f.findField('T2P3').getValue(),
                    P4: f.findField('T2P4').getValue(),
                    P5: f.findField('T2P5').getValue(),
                    P6: f.findField('T2P6').getValue()['TC_TYPE']
                };
                Ext.apply(store.proxy.extraParams, np);

            },
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/GA0005/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
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
    function T11Load() {
        T11Store.load({
            params: {
                start: 0
            }
        });
    }
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: T1LastRec.data['PUR_NO'] });
                            p.push({ name: 'P1', value: T1Query.getForm().findField('T1P4').getValue()['TC_TYPE'] });
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }
            //,
            //{
            //    id: 't1print', text: '列印', disabled: false, handler: function () {
            //        showReport();
            //    }
            //}
        ]
    });
    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
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
                dataIndex: 'PUR_NO',
                width: 100
            }, {
                text: "訂購日期",
                dataIndex: 'PUR_DATE',
                width: 70
            }, {
                text: "藥品種類",
                dataIndex: 'TC_TYPE',
                width: 100,
                renderer: function (val, meta, record) {
                    if (val == 'A')
                        return 'A 科學中藥';
                    else if (val == 'B')
                        return 'B 飲片';
                    else
                        return val;
                }
            }, {
                text: "訂購人",
                dataIndex: 'PUR_UNM',
                width: 100
            }, {
                text: "訂購單備註",
                dataIndex: 'PUR_NOTE',
                width: 150
            }, {
                text: "訂單狀態",
                dataIndex: 'PURCH_ST',
                width: 100
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    // T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    // T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1Rec > 0)
                {
                    T11Query.down('#btnT11Query').setDisabled(false);
                    T11Load();
                }   
            }
        }
    });
    var T11Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T11Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "電腦編碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 120
            }, {
                text: "藥商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 150
            }, {
                text: "訂購數",
                dataIndex: 'PUR_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "單位劑量",
                dataIndex: 'PUR_UNIT',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "進貨單價",
                dataIndex: 'IN_PURPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "金額小計",
                dataIndex: 'PUR_AMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T11Query.down('#btnT11Query').setDisabled(T11Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "訂單編號",
                dataIndex: 'PUR_NO',
                width: 100
            }, {
                text: "訂購日期",
                dataIndex: 'PUR_DATE',
                width: 70
            }, {
                text: "藥品種類",
                dataIndex: 'TC_TYPE',
                width: 100,
                renderer: function (val, meta, record) {
                    if (val == 'A') 
                        return 'A 科學中藥';
                    else if (val == 'B')
                        return 'B 飲片';
                    else
                        return val;
                }
            }, {
                text: "訂購人",
                dataIndex: 'PUR_UNM',
                width: 100
            }, {
                text: "訂購單備註",
                dataIndex: 'PUR_NOTE',
                width: 150
            }, {
                text: "訂單狀態",
                dataIndex: 'PURCH_ST',
                width: 100
            }, {
                text: "電腦編碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 120
            }, {
                text: "藥商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 150
            }, {
                text: "訂購量",
                dataIndex: 'PUR_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "單位劑量",
                dataIndex: 'PUR_UNIT',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "進貨單價",
                dataIndex: 'IN_PURPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "金額小計",
                dataIndex: 'PUR_AMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 90
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {

                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
            }
        }
    });

    //function showReport() {
    //    if (!win) {
    //        var winform = Ext.create('Ext.form.Panel', {
    //            id: 'iframeReport',
    //            layout: 'fit',
    //            closable: false,
    //            html: '<iframe src="' + reportUrl + '?MAT_CLASS=' + P0 + '&DIS_TIME_B=' + Ext.Date.format(P1, 'Y/m/d') + '&DIS_TIME_E=' + Ext.Date.format(P2, 'Y/m/d') + '&P3=' + P3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
    //            buttons: [{
    //                text: '關閉',
    //                handler: function () {
    //                    this.up('window').destroy();
    //                }
    //            }]
    //        });
    //        var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

    //    }
    //    win.show();
    //}

    var vHeight = window.screen.availHeight; // 解析度高度
    var Tabs = Ext.widget('tabpanel', {
        region: 'center',
        layout: 'fit',
        collapsible: false,
        title: '',
        border: false,
        items: [{
            itemId: 't1Grid',
            title: '兩層顯示',
            items: [{
                itemId: 't1GridSub',
                region: 'north',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                height: vHeight * 0.3,
                split: true,
                items: [T1Grid]
            }, {
                itemId: 't11GridSub',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                //height: vHeight * 0.3,
                split: true,
                items: [T11Grid]
            }]
        }, {
            itemId: 't2Grid',
            title: '單層顯示',
            items: [{
                itemId: 't2GridSub',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                //height: vHeight * 0.6,
                split: true,
                items: [T2Grid]
            }]
        }],
        listeners: {
            // 切換tab後,radiogroup的取值會抓不到,所以在切換tab前(beforetabchange)記錄選取值,換回來後(tabchange)重設選取的值
            beforetabchange: function (panel, newTab, oldTab, eOpts) {
                if (newTab.itemId == 't1Grid')
                    T2tmpTC_TYPE = T2Query.getForm().findField('T2P6').getValue()['TC_TYPE'];
                else if (newTab.itemId == 't2Grid')
                    T1tmpTC_TYPE = T1Query.getForm().findField('T1P4').getValue()['TC_TYPE'];
            },
            tabchange: function (panel, newTab, oldTab, eOpts) {
                if (newTab.itemId == 't1Grid') {
                    T1Query.getForm().findField('T1P4').setValue({
                        TC_TYPE: T1tmpTC_TYPE
                    });
                }
                else if (newTab.itemId == 't2Grid') {
                    T2Query.getForm().findField('T2P6').setValue({
                        TC_TYPE: T2tmpTC_TYPE
                    });
                }
            }
        }
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [Tabs]
    });
});
