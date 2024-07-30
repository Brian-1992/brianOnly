Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "點選申請單編號";
    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Query = Ext.widget({
        title: '查詢',
        itemId: 'form',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 35%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            plugins: 'responsive',
            layout: {
                type: 'hbox',
                vertical: false
            },
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        width: '30%'
                    }
                },
                'width >= 600': {
                    layout: {
                        width: '30%'
                    }
                }
            },
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '物料條碼',
                    name: 'P1',
                    id: 'P1',
                    width: '90%',
                    enforceMaxLength: true
                }, {
                    xtype: 'button',
                    itemId: 'scanBtn',
                    iconCls: 'TRACamera',
                    width: '9%',
                    handler: function () {
                        showScanWin(viewport);
                    }
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'box',
                        vertical: true,
                        align: 'stretch'
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'box',
                        vertical: false
                    }
                }
            },
            items: [
                {
                    xtype: 'button',
                    text: '查詢',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            height: 40,
                            width: '50%'
                        },
                    },
                    margin: '1',
                    handler: function () {
                        var f = this.up('form').getForm();
                        if (!f.isValid())
                            Ext.Msg.alert('提醒', '查詢條件為必填');
                        else {
                            T1Load();
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            height: 40,
                            width: '50%'
                        },
                    },
                    margin: '1',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P1').focus();
                    }
                }
            ]
        }]
    });
    
    // 供相機掃描取得的條碼
    Ext.define('CameraSharedData', {
        callSubmitBack: function () {
            T1Query.getForm().findField('P1').setValue(CameraSharedData.selItem);
        },
        selItem: '',
        selQty: '',
        singleton: true    //no singleton, no return
    });

    var T1Store = Ext.create('WEBAPP.store.CC0004', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                    T1Tool.onLoad();
                }
                else if (records.length < 1) {
                    Ext.Msg.alert('提醒', '無法找到對應品項!!!');
                }
                else if (records.length == 1) {
                    // 查詢結果只有一筆,直接跳出填寫視窗
                    T1LastRec = records[0];
                    popP11ItemDetail();
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        plugins: 'bufferedrenderer',
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Query]
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    var callableWin = null;
    popP11ItemDetail = function () {
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'itemDetail',
                height: '100%',
                layout: 'fit',
                closable: false,
                border: true,
                fieldDefaults: {
                    labelAlign: 'right',
                    labelWidth: 100,
                    width: '100%'
                },
                items: [
                    {
                        xtype: 'container',
                        layout: 'vbox',
                        padding: '2vmin',
                        scrollable: true,
                        items: [
                            {
                                xtype: 'displayfield',
                                fieldLabel: '院內碼',
                                name: 'MMCODE',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '中文品名',
                                name: 'MMNAME_C',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '英文品名',
                                name: 'MMNAME_E',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠商碼',
                                name: 'M_AGENNO',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠商中文名稱',
                                name: 'AGEN_NAMEC',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠牌',
                                name: 'M_AGENLAB',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '儲位',
                                name: 'STORE_LOC',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '最小計量單位',
                                name: 'BASE_UNIT',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '轉換率',
                                name: 'EXCH_RATIO',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '包裝單位',
                                name: 'M_PURUN',
                                readOnly: true
                            }
                        ]
                    }
                ],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popform, '品項基本資料查詢', viewport.width * 0.95, viewport.height * 0.95);
        }
        callableWin.show();
        popform.loadRecord(T1LastRec);
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
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    Ext.getDoc().dom.title = T1Name;
});
