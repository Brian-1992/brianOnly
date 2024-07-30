Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = '中央庫房庫存成本調整報表'
    var reportUrl = '/Report/F/FA0008.aspx';
    var YMComboGet = '../../../api/FA0008/GetYMCombo';
    var T1LastRec = null;

    // 成本年月清單
    var YMQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
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

    var T1Store = Ext.create('WEBAPP.store.FA0008VM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T1Query.getForm().findField('P1').rawValue,
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
        autoScroll: true, //若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                            labelWidth: 60,
                            width: 140,
                            padding: '0 4 0 4',
                            fieldCls: 'required',
                            store: YMQueryStore,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            autoSelect: true,
                            listeners: {
                                change: function () {
                                    Ext.getCmp('t1print').setDisabled(true);
                                }
                            },
                            editable: false
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
                            }
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '',
                            value: '(公式：A+B-C+D-E=F)',
                            padding:'0 0 0 20'
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
                    //if (T1Store.getCount() > 0)
                    //    showReport();
                    //else
                    //    Ext.Msg.alert('訊息', '請先建立報表資料。');
                    showReport();
                }
            },
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
                text: "類別",
                dataIndex: 'MAT',
                renderer: function (value) {
                    switch (value) {
                        case '小計': return '<span style="color:black; font-weight:bold">小計</span>'; break;
                        case '一般物品小計': return '<span style="color:black; font-weight:bold">一般物品小計</span>'; break;
                        default: return value; break;
                    }
                },
                width: 130
            },
            {
                text: '期初庫存成本(A)',
                columns: [
                    {
                        text: "中央庫房",
                        dataIndex: 'PIQ_PA_G1',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    }, {
                        text: "衛星庫房",
                        dataIndex: 'PIQ_PA_G2',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    }]
            },
            {
                text: '進貨成本(B)',
                columns: [
                    {
                        text: "中央庫房",
                        dataIndex: 'IQ_IP',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    }]
            },
            {
                text: '消耗成本(C)',
                columns: [
                    {
                        text: "中央庫房",
                        dataIndex: 'OQ_A_AP_G1',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    },
                    {
                        text: "衛星庫房",
                        dataIndex: 'OQ_A_AP_G2',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    }]
            },
            {
                text: '盤盈虧(D)',
                columns: [
                    {
                        text: '中央庫房',
                        columns: [
                            {
                                text: "盤盈",
                                dataIndex: 'I_AP_G1_P',
                                style: 'text-align:center',
                                width: 80, align: 'right'
                            },
                            {
                                text: "盤虧",
                                dataIndex: 'I_AP_G1_N',
                                style: 'text-align:center',
                                width: 80, align: 'right'
                            },
                        ]

                    },
                    {
                        text: '衛星庫房',
                        columns: [
                            {
                                text: "盤盈",
                                dataIndex: 'I_AP_G2_P',
                                style: 'text-align:center',
                                width: 80, align: 'right'
                            },
                            {
                                text: "盤虧",
                                dataIndex: 'I_AP_G2_N',
                                style: 'text-align:center',
                                width: 80, align: 'right'
                            },
                        ]

                    },
                    
                    //{
                    //    text: "衛星庫房盤盈",
                    //    dataIndex: 'I_AP_G2_P',
                    //    style: 'text-align:center',
                    //    width: 80, align: 'right'
                    //}
                    //,
                    //{
                    //    text: "衛星庫房盤虧",
                    //    dataIndex: 'I_AP_G2_N',
                    //    style: 'text-align:center',
                    //    width: 80, align: 'right'
                    //}
                ]
            },
            {
                text: '台北門診應收帳款(E)',
                columns: [
                    {
                        text: "衛星庫房",
                        dataIndex: 'A_PA',
                        style: 'text-align:center',
                        width: 120, align: 'right'
                    }]
            },
            {
                text: '期末庫存成本(F)',
                columns: [
                    {
                        text: "中央庫房",
                        dataIndex: 'IQ_PA_G1',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    },
                    {
                        text: "衛星庫房",
                        dataIndex: 'IQ_PA_G2',
                        style: 'text-align:center',
                        width: 80, align: 'right'
                    }]
            },
            {
                text: "寄售衛材消耗成本(G)",
                dataIndex: 'A_PC',
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
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];

                if (T1LastRec != null) {
                    Ext.getCmp('t1print').setDisabled(false);
                }
            }
        }
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?DATA_YM=' + T1Query.getForm().findField('P1').rawValue
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

    Ext.getCmp('t1print').setDisabled(true);
});