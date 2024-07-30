
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "應付廠商金額明細總表";
    var UrlReport = '/Report/B/BG0011.aspx';
    //var T1GetExcel = '/api/FA0080/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_class = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/BG0011/GetClassCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var classsubs = data.etts;
                    if (classsubs.length > 0) {
                        for (var i = 0; i < classsubs.length; i++) {
                            st_class.add({ VALUE: classsubs[i].VALUE, TEXT: classsubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QueryAgenNoCombo = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P4',
        id: 'P4',
        fieldLabel: '廠商代碼',
        labelAlign: 'right',
        //fieldCls: 'required',
        allowBlank: false,
        labelWidth: 80,
        width: 290,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/BG0011/GetAgenNoCombo',//指定查詢的Controller路徑
        extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    var T1QueryAgenNoCombo_1 = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P5',
        id: 'P5',
        fieldLabel: '至',
        labelAlign: 'right',
        //fieldCls: 'required',
        allowBlank: false,
        labelWidth: 20,
        width: 230,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/BG0011/GetAgenNoCombo_1',//指定查詢的Controller路徑
        extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        getDefaultParams: function () {//查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 70,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        id: 'PanelP1',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'monthfield',
                                fieldLabel: '查詢月份',
                                name: 'P0',
                                id: 'P0',
                                labelWidth: 80,
                                width: 180,
                                fieldCls: 'required',
                                value: new Date(),
                                allowBlank: false
                            },
                            //{
                            //    xtype: 'monthfield',
                            //    fieldLabel: '~',
                            //    name: 'P1',
                            //    id: 'P1',
                            //    labelSeparator: '',
                            //    enforceMaxLength: true,
                            //    labelWidth: 10,
                            //    width: 110,
                            //    fieldCls: 'required',
                            //    value: new Date(),
                            //    allowBlank: false
                            //},
                            {
                                xtype: 'combo',
                                fieldLabel: '類別',
                                name: 'P2',
                                id: 'P2',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_class,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    },
                    {
                        xtype: 'panel',
                        id: 'PanelP2',
                        border: false,
                        layout: 'hbox',
                        items: [
                            T1QueryAgenNoCombo,
                            T1QueryAgenNoCombo_1,
                            {
                                xtype: 'button',
                                text: '查詢',
                                id: 'T1btn1',
                                handler: function () {
                                    if (T1Query.getForm().findField("P0").getValue() == null || T1Query.getForm().findField("P0").getValue() == ""
                                        //|| T1Query.getForm().findField("P1").getValue() == null || T1Query.getForm().findField("P1").getValue() == ""
                                    ) {
                                        Ext.Msg.alert('提醒', '年月、庫房及藥材不可空白');
                                    }
                                    else {
                                        T1Load();
                                    }
                                }
                            }, {
                                xtype: 'button',
                                text: '清除',
                                handler: function () {
                                    var f = this.up('form').getForm();
                                    f.reset();
                                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                }
                            }
                        ]
                    }
                ]
            }

        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },
            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            //{ name: 'F9', type: 'string' },
            //{ name: 'F10', type: 'string' }
        ]

    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0011/All',
            //timeout: 9000000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    //p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    var dataCount = store.getCount();
                    if (dataCount > 0) {
                        T1Tool.down('#print1').setDisabled(false);
                        T1Tool.down('#print2').setDisabled(false);
                        T1Tool.down('#print3').setDisabled(false);
                        //if (T1Query.getForm().findField('P2').getValue() == '1' || T1Query.getForm().findField('P2').getValue() == '2') {
                        //    T1Tool.down('#print2').setDisabled(false);
                        //    T1Tool.down('#print3').setDisabled(false);
                        //}
                    }
                    else {
                        T1Tool.down('#print1').setDisabled(true);
                        T1Tool.down('#print2').setDisabled(true);
                        T1Tool.down('#print3').setDisabled(true);
                    }
                }
            }
        }
    });

    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print1', text: '列印應付廠商金額明細總表', disabled: true,
                handler: function () {
                    showReport();
                }
            },
            {
                itemId: 'print2', text: '列印支出憑證黏存單', disabled: true,
                handler: function () {
                    showReport2();
                }
            },
            {
                itemId: 'print3', text: '列印支出憑證黏存單(A3)', disabled: true,
                handler: function () {
                    showReport3();
                }
            }

        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        //loadingText: '處理中...',
        //loadMask: true,
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
            { text: "項次", dataIndex: 'rnm', width: 60 },
            { text: "廠商代碼", dataIndex: 'F1', width: 100 },
            { text: "廠商名稱", dataIndex: 'F2', width: 100, style: 'text-align:left', align: 'center' },
            { text: "發票號碼", dataIndex: 'F3', width: 100, style: 'text-align:left', align: 'right' },
            { text: "統一編號", dataIndex: 'F4', width: 100, style: 'text-align:left', align: 'right' },
            { text: "實付金額", dataIndex: 'F5', width: 100, style: 'text-align:left', align: 'right' },
            { text: "合約優惠", dataIndex: 'F6', width: 100, style: 'text-align:left', align: 'right' },
            { text: "應付金額", dataIndex: 'F7', width: 100, style: 'text-align:left', align: 'right' },
            //{ text: "差異", dataIndex: 'F8', width: 100, style: 'text-align:left', align: 'right' },
            //{ text: "結存", dataIndex: 'F9', width: 100, style: 'text-align:left', align: 'right' },
            //{ text: "進出單位", dataIndex: 'F10', width: 200 },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //T1Rec = records.length;
                //T1LastRec = records[0];
            }
        }
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?rdlc=1'
                + '&P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P2=' + T1Query.getForm().findField('P2').getValue()
                + '&P4=' + T1Query.getForm().findField('P4').rawValue
                + '&P5=' + T1Query.getForm().findField('P5').rawValue
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    function showReport2() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?rdlc=2'
                + '&P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P2=' + T1Query.getForm().findField('P2').getValue()
                + '&P4=' + T1Query.getForm().findField('P4').rawValue
                + '&P5=' + T1Query.getForm().findField('P5').rawValue
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }

    function showReport3() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?rdlc=3'
                + '&P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P2=' + T1Query.getForm().findField('P2').getValue()
                + '&P4=' + T1Query.getForm().findField('P4').rawValue
                + '&P5=' + T1Query.getForm().findField('P5').rawValue
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

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
            split: true
        },
        items: [
            {
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                items: [T1Grid]
            }
        ]
    });
});