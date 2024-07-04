Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "品項基本資料查詢";

    var T1Rec = 0;
    var T1LastRec = null;
    var GetCLSNAME = '/api/CB0008/GetCLSNAME';
    // 物料分類
    var CLSNAMEStore = Ext.create('Ext.data.Store', {  //物料分類的store
        fields: ['VALUE', 'TEXT']
    });
    function SetCLSNAME() { //建立物料分類的下拉式選單
        Ext.Ajax.request({
            url: GetCLSNAME,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var clsnames = data.etts;
                    if (clsnames.length > 0) {
                        for (var i = 0; i < clsnames.length; i++) {
                            CLSNAMEStore.add({ VALUE: clsnames[i].VALUE, TEXT: clsnames[i].TEXT });
                        }
                        T1Query.getForm().findField("P5").setValue(clsnames[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    SetCLSNAME();
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 供相機掃描取得的條碼
    Ext.define('CameraSharedData', {
        callSubmitBack: function () {
            T1Form.getForm().findField('P2').setValue(CameraSharedData.selItem);
        },
        selItem: '',
        selQty: '',
        singleton: true    //no singleton, no return
    });

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
            labelStyle: 'width: 45%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
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
                    xtype: 'textfield',
                    fieldLabel: '庫房別',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true,
                    maxLength: 6,
                    padding: '0 4 0 4',
                    allowBlank: false,
                    fieldCls: 'required',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 45%',
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P5',
                    id: 'P5',
                    enforceMaxLength: true,
                    maxLength: 2,
                    padding: '0 4 0 4',
                    plugins: 'responsive',
                    store: CLSNAMEStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 45%',
                        }
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 13,
                    padding: '0 4 0 4',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 45%',
                        }
                    }
                }, {
                    xtype: 'panel',
                    border: false,
                    padding: '0 4 0 4',
                    plugins: 'responsive',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    responsiveConfig: {
                        'width < 600': {
                            width: '70%'
                        },
                        'width >= 600': {
                            width: '30%'
                        }
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '品項條碼',
                            name: 'P2',
                            id: 'P2',
                            fieldCls: 'required',
                            allowBlank: false,
                            enforceMaxLength: true,
                            maxLength: 200,
                            width: '90%',
                            margin: '2 4 0 4',
                            plugins: 'responsive',
                            responsiveConfig: {
                                'width < 600': {
                                    labelStyle: 'width: 33%',
                                },
                                'width >= 600': {
                                    labelStyle: 'width: 45%',
                                }
                            },
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length >= 8) {
                                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                        T1Load();
                                    }
                                }
                            }
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
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '1 4 0 4',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 45%',
                        }
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 45%',
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = this.up('form').getForm();
                        if (f.findField('P0').getValue() == '')
                            Ext.Msg.alert('提醒', '查詢條件的庫房別為必填');
                        else {
                            Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                            T1Load();
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.CB0008', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
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
        //title: T1Name,
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
        columns: [{
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: '25%'
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: '70%'
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            },
            itemclick: function (view, record, item, index, e, eOpts) {
                popItemDetail();
            }
        }
    });

    var callableWin = null;
    popItemDetail = function () {
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'itemDetail',
                height: '100%',
                layout: 'fit',
                closable: false,
                border: true,
                fieldDefaults: {
                    labelAlign: 'right',
                    labelWidth: 130,
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
                                fieldLabel: '庫房別',
                                name: 'WH_NO',
                                readOnly: true
                            }, {
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
                                fieldLabel: '庫備別',
                                name: 'M_STOREID',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '存量',
                                name: 'INV_QTY',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '品項條碼',
                                name: 'BARCODE',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠商',
                                name: 'M_AGENNO',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠牌',
                                name: 'M_AGENLAB',
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

            callableWin = GetPopWin(viewport, popform, '品項資料明細', viewport.width * 0.9, viewport.height * 0.8);
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

    //T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P0').setValue(session['Inid']);
    T1Query.getForm().findField('P2').focus();

    Ext.getDoc().dom.title = T1Name;

});
