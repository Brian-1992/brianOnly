﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.Ajax.setTimeout(1200000);
    var T1LastRec = null;

    var viewModel = Ext.create('WEBAPP.store.AA.AA0163VM');
    var T1Store = viewModel.getStore('AA0163');
    T1Store.on({
        load: function (store, options) {   //設定匯入,列印是否disable
            var dataCount = store.getCount();
            if (dataCount > 0) {
                Ext.getCmp('print').setDisabled(false);
            } else {
                Ext.getCmp('print').setDisabled(true);
            }
        }
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("DATA_YM", T1Query.getForm().findField('DATA_YM').getValue());
        T1Store.getProxy().setExtraParam("MONEY1", T1Query.getForm().findField('MONEY1').getValue());
        T1Store.getProxy().setExtraParam("MONEY2", T1Query.getForm().findField('MONEY2').getValue());
        T1Tool.moveFirst();
    }

    var YmStore = viewModel.getStore('YM');
    function YmLoad() {
        YmStore.load({
            params: {
                start: 0
            }
        });
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
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'numberfield',
                        fieldLabel: '藥品當月醫療總收入金額',
                        name: 'MONEY1',
                        fieldCls: 'required',
                        allowBlank: false,
                        minValue: 0,
                        labelWidth: 150,
                        width: 250,
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '消耗性醫療器材當月醫療總收入金額',
                        name: 'MONEY2',
                        fieldCls: 'required',
                        allowBlank: false,
                        minValue: 0,
                        padding: '0 0 0 10',
                        labelWidth: 200,
                        width: 300,
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '資料年月',
                        store: YmStore,
                        name: 'DATA_YM',
                        id: 'DATA_YM',
                        labelWidth: 80,
                        width: 180,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE'
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        style: 'margin:0px 5px 0px 20px;',
                        handler: function () {
                            var f = T1Query.getForm();
                            if (f.isValid()) {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                myMask.show();
                                T1Load();
                                myMask.hide();
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        style: 'margin:0px 5px;',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            },
        ]
    });


    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print',
                id: 'print',
                name: 'print',
                text: '列印',
                style: 'margin:0px 5px;',
                handler: function () {
                    showReport();
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            T1Store.getProxy().setExtraParam("DATA_YM", T1Query.getForm().findField('DATA_YM').getValue());
            T1Store.getProxy().setExtraParam("MONEY1", T1Query.getForm().findField('MONEY1').getValue());
            T1Store.getProxy().setExtraParam("MONEY2", T1Query.getForm().findField('MONEY2').getValue());

            var np = {
                p0: T1Query.getForm().findField('DATA_YM').getValue(),
                p1: T1Query.getForm().findField('MONEY1').getValue(),
                p2: T1Query.getForm().findField('MONEY2').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="/Report/A/AA0163.aspx?DATA_YM=' + np.p0 + '&MONEY1=' + np.p1 + '&MONEY2=' + np.p2 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                text: "統計分類",
                dataIndex: 'ITEM_NAME',
                style: 'text-align:left',
                align: 'left',
                width: 200
            },
            {
                text: "藥品類",
                dataIndex: 'AMT_1',
                style: 'text-align:left',
                align: 'right',
                width: 150
            },
            {
                text: "消耗性醫療器材類",
                dataIndex: 'AMT_2',
                style: 'text-align:left',
                align: 'right',
                width: 150
            },
            {
                text: "備考",
                dataIndex: 'REMARK',
                style: 'text-align:left',
                align: 'right',
                width: 200
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
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
        items: [
            {
                region: 'north',
                layout: 'fit',
                collapsible: false,
                title: '',
                //height: 50,
                layout: 'fit',
                items: [T1Query]
            },
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            }
        ]
    });
    Ext.getCmp('print').setDisabled(true);
});