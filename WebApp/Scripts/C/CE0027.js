Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';

    var reportUrl = '/Report/C/CE0027.aspx';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();
    var T1cell = '';
    var T1LastRec = null;
    var countData = null;

    var viewModel = Ext.create('WEBAPP.store.CE0027VM');

    var T1Store = viewModel.getStore('MasterAll');
    function T1Load(clearMsg) {

        if (clearMsg) {
            msglabel('');
        }

        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue()['P1']);
        T1Tool.moveFirst();
    }

    function T2Load() {
        T2Store.loadPage();
    }

    function getChkPeriod() {
        Ext.Ajax.request({
            url: '/api/CE0027/GetChkPeriod',
            method: reqVal_p,
            params: { chk_ym: T1Query.getForm().findField('P0').rawValue },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('P2').setValue(data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getAploutqtyDateRange() {
        Ext.Ajax.request({
            url: '/api/CE0024/AploutqtyDateRange',
            method: reqVal_p,
            params: { chk_ym: T1Query.getForm().findField('P0').rawValue },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('P3').setValue('<span style="color:blue">' + data.msg + '</span>');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'WEBAPP.model.CE0027',
        pageSize: 20,
        remoteSort: true,
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    setT1Form(store.data.items[0].data);
                }
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                }
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CE0027/GetDetails',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
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
            width: 250
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
                            fieldLabel: '盤點年月',
                            name: 'P0',
                            id: 'P0',
                            labelWidth: mLabelWidth,
                            width: 240,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls:'required',
                            value: new Date(),
                            listeners: {
                                change: function (field, newValue, oldValue) {

                                }
                            }
                        },
                        {
                            xtype: 'radiogroup',
                            name: 'P1',
                            fieldLabel: '顯示內容',
                            labelAlign: 'right',
                            items: [
                                { boxLabel: '全部', width: 60, name: 'P1', inputValue: '0', checked: true },
                                { boxLabel: '盤盈', width: 60, name: 'P1', inputValue: '1' },
                                { boxLabel: '盤虧', width: 60, name: 'P1', inputValue: '2' }
                            ],
                            listeners: {
                                change: function (field, newValue, oldValue) {

                                }
                            }
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue() &&
                                    !f.findField('P1').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>盤點年月</span>與<span style=\'color:red\'>顯示內容</span>需填寫');
                                    return;
                                }

                                T1Load(true);
                                T2Load();
                                getChkPeriod();
                                getAploutqtyDateRange();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '本次盤點期',
                            name: 'P2',
                            id: 'P2',
                            value: '',
                            width: 130
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '',
                            name: 'P3',
                            id: 'P3',
                            value: '',
                            width: 230
                        },
                    ]
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                itemId: 'btnPrint',
                text: '列印',
                handler: function () {
                    showReport();
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
                width: 100
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 220
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 220
            },
            {
                text: "庫存量",
                dataIndex: 'STORE_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 80
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "誤差量",
                dataIndex: 'GAP_T',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "移動平均價",
                dataIndex: 'AVG_PRICE',
                align: 'right',
                style: 'text-align:left',
                width: 90
            },
            {
                text: "誤差金額",
                dataIndex: 'DIFF_COST',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "誤差百分比",
                dataIndex: 'DIFF_P',
                align: 'right',
                style: 'text-align:left',
                width: 90
            },
            {
                text: "消耗量",
                dataIndex: 'APL_OUTQTY',
                align: 'right',
                style: 'text-align:left',
                width: 70
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 200,
                flex: 1
            }
        ]
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 2
        },
        frame: false,
        title: '',
        bodyPadding: '5 5',
        border: false,
        defaultType: 'displayfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: "right"
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3,
                    tableAttrs: {
                        style: {
                            width: '100%'
                        }
                    }
                },
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項現品量總金額',
                        labelWidth: 130,
                        name: 'TOT1',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項現品量總金額',
                        labelWidth: 130,
                        name: 'P_TOT1',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項現品量總金額',
                        labelWidth: 130,
                        name: 'N_TOT1',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項電腦量總金額',
                        labelWidth: 130,
                        name: 'TOT2',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項電腦量總金額',
                        labelWidth: 130,
                        name: 'P_TOT2',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項電腦量總金額',
                        labelWidth: 130,
                        name: 'N_TOT2',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項誤差百分比',
                        labelWidth: 130,
                        name: 'TOT3',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項誤差百分比',
                        labelWidth: 130,
                        name: 'P_TOT3',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項誤差百分比',
                        labelWidth: 130,
                        name: 'N_TOT3',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項誤差總金額',
                        labelWidth: 130,
                        name: 'TOT4',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項誤差總金額',
                        labelWidth: 130,
                        name: 'P_TOT4',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項誤差總金額',
                        labelWidth: 130,
                        name: 'N_TOT4',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤總品項當季耗總金額',
                        labelWidth: 130,
                        name: 'TOT5',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤盈品項當季耗總金額',
                        labelWidth: 130,
                        name: 'P_TOT5',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '盤虧品項當季耗總金額',
                        labelWidth: 130,
                        name: 'N_TOT5',
                    }
                ]
            }]
    });

    function setT1Form(data) {
        var f = T1Form.getForm();
        f.findField('TOT1').setValue(data.TOT1);
        f.findField('TOT2').setValue(data.TOT2);
        f.findField('TOT3').setValue(data.TOT3);
        f.findField('TOT4').setValue(data.TOT4);
        f.findField('TOT5').setValue(data.TOT5);
        f.findField('P_TOT1').setValue(data.P_TOT1);
        f.findField('P_TOT2').setValue(data.P_TOT2);
        f.findField('P_TOT3').setValue(data.P_TOT3);
        f.findField('P_TOT4').setValue(data.P_TOT4);
        f.findField('P_TOT5').setValue(data.P_TOT5);
        f.findField('N_TOT1').setValue(data.N_TOT1);
        f.findField('N_TOT2').setValue(data.N_TOT2);
        f.findField('N_TOT3').setValue(data.N_TOT3);
        f.findField('N_TOT4').setValue(data.N_TOT4);
        f.findField('N_TOT5').setValue(data.N_TOT5);
    }

    function showReport(record) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + T1Query.getForm().findField('P0').rawValue
                + '&p1=' + T1Query.getForm().findField('P1').getValue()['P1']
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 50, viewport.height - 20);
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
                items: [T1Grid]
            },
            {
                itemId: 'form',
                region: 'south',
                collapsible: false,
                floatable: true,
                split: true,
                width: '20%',
                minWidth: 120,
                minHeight: 140,
                collapsed: false,
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }
        ]
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    T1Load(true);
    T2Load();
    getChkPeriod();
    getAploutqtyDateRange();
});