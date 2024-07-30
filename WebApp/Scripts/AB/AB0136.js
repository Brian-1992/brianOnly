Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "寄售數量回報";
    var qtySet = '/api/AB0136/SetQty';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var whnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AB0136/GetWhnoCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var storeItems = data.etts;
                    if (storeItems.length > 0) {
                        for (var i = 0; i < storeItems.length; i++) {
                            whnoStore.add({ VALUE: storeItems[i].VALUE, TEXT: storeItems[i].TEXT, COMBITEM: storeItems[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '藥材代碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 170,
        matchFieldWidth: false,
        listConfig: { width: 200 },
        limit: 20, //限制一次最多顯示20筆
        padding: '0 0 0 4',
        queryUrl: '/api/AB0136/GetMMCODECombo' //指定查詢的Controller路徑
    });

    var mLabelWidth = 60;
    var mWidth = 180;
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
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '庫房',
                    store: whnoStore,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 60,
                    width: 200,
                    anyMatch: true,
                    matchFieldWidth: false,
                    listConfig: { width: 300 },
                }, T1QuryMMCode,
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'WH_NAME', 'MMCODE', 'MMNAME_C ', 'MMNAME_E', 'QTY', 'RQTY', 'DISC_CPRICE']
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Tool.down('#add').setDisabled(false);
                    }
                    else {
                        T1Tool.down('#add').setDisabled(true);
                        msglabel('查無資料');
                    }
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0136/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
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
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '儲存', disabled: true, handler: function () {
                    var store = T1Grid.getStore().data.items;

                    Ext.MessageBox.confirm('儲存', '是否確定儲存?', function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var list = [];
                            for (var i = 0; i < store.length; i++) {
                                // 有修改的才處理
                                if (store[i].dirty) {
                                    var item = {
                                        WH_NO: store[i].data.WH_NO,
                                        MMCODE: store[i].data.MMCODE,
                                        RQTY: store[i].data.RQTY
                                    }
                                    list.push(item);
                                }
                            }
                            if (list.length == 0) {
                                Ext.Msg.alert('訊息', '尚未填寫任何數量資料，請確認!');
                                myMask.hide();
                                return;
                            }
                            Ext.Ajax.request({
                                url: qtySet,
                                method: reqVal_p,
                                params: {
                                    list: Ext.util.JSON.encode(list)
                                },
                                success: function (response) {
                                    myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel("儲存完成");
                                        T1Load();
                                    } else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                    }
                                },
                                failure: function (response, options) {
                                    myMask.hide();
                                }
                            });
                        }
                    }
                    );
                }
            },
            {
                itemId: 'T1cancel', id: 'T1cancel', text: '取消', handler: function () {
                    T1Load();
                }
            }
        ]
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
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房",
            dataIndex: 'WH_NAME',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "藥材代碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "<b><font color=red>數量</font></b>",
            dataIndex: 'RQTY',
            style: 'text-align:left',
            align: 'right',
            width: 80,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/ // 用正規表示式限制可輸入內容
            }
        }, {
            text: "單價",
            dataIndex: 'DISC_CPRICE',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                    },
                    validateedit: function (editor, context, eOpts) {
                    }
                }
            })
        ],
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
        items: [
            {
                itemId: 'tGrid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [{
                    region: 'center',
                    layout: {
                        type: 'border',
                        padding: 0
                    },
                    collapsible: false,
                    title: '',
                    split: true,
                    width: '80%',
                    flex: 1,
                    minWidth: 50,
                    minHeight: 140,
                    items: [
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
                }]
            }
        ]
    });
    T1Query.getForm().findField('P0').focus();
});
