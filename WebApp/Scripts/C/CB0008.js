Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "品項基本資料查詢";
    var GetCLSNAME = '/api/CB0008/GetCLSNAME';
    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //申請單位Store
    var st_WHNO = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CB0008/GetWHNOCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
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

    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 0px;',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '庫房別',
                    store: st_WHNO,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    anyMatch: true,
                    queryMode: 'local',
                    name: 'P0',
                    id: 'P0',
                    padding: '0 4 0 4',
                    width: 350,
                    allowBlank: false,
                    fieldCls: 'required'
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P5',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 170,
                    padding: '0 4 0 4',
                    store: CLSNAMEStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 13,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '品項條碼',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 0px;',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    maxLength: 200,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var f = this.up('form').getForm();
                        if (f.findField('P0').getValue() == '')
                            Ext.Msg.alert('提醒', '查詢條件的庫房別為必填');
                        else
                            T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
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
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            layout: 'fit',
            xtype: 'toolbar',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房別",
            dataIndex: 'WH_NO',
            width: 60
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200
        }, {
            text: "庫備別",
            dataIndex: 'M_STOREID',
            width: 60
        }, {
            text: "存量",
            dataIndex: 'INV_QTY',
            width: 60,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "品項條碼",
            dataIndex: 'BARCODE',
            width: 200
        }, {
            text: "儲位",
            dataIndex: 'STORE_LOC',
            width: 200
        }, {
            text: "廠商",
            dataIndex: 'M_AGENNO',
            width: 'M_AGENNO',
            width: 100
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
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

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P0').setValue(session['Inid']);
});
