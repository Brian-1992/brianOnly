Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Name = '盤點輸入作業';
    var T1F1 = '';
    var T1F2 = '';
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1LastRec = null;
    var T1Rec = 0;

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0171/GetMmCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        T1QueryMMCode,
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1btn1',
                            handler: function () {
                                msglabel('訊息區:');
                                T1F1 = '';
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
                                T1Load();
                                T1F1 = '';
                                T1Grid.down('#T1update').setDisabled(true);
                            }
                        }
                    ]
                }
            ]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' 
            },
            url: '/api/AA0171/AllM',
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
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
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
                id: 'T1update',
                itemId: 'T1update', text: '庫存轉入藥局', disabled: true, handler: function () {
                    
                    var selection = T1Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let name = '';
                        let mmcode = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('MMCODE') + '」<br>';
                            mmcode += item.get('MMCODE') + ',';
                        })
                        Ext.MessageBox.confirm('庫存轉入藥局', '是否確定庫存轉入藥局？院內碼如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0171/Apply',
                                    method: reqVal_p,
                                    params: {
                                        MMCODE: mmcode
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('訊息區:庫存轉入藥局成功');
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        }
                        );
                    }
                    
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
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            },
            listeners: {
                selectionchange: function (selectionModel, selected, options) {
                    T1Grid.down('#T1update').setDisabled(true);
                    for (var i = 0; i < selected.length; i++) {
                        if (Number(selected[i].data.INV_QTY) > 0) {
                            T1Grid.down('#T1update').setDisabled(false);
                        }
                    }
                },
            }
        }),
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer',
            width: 50
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 120
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 180
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "庫存量",
            dataIndex: 'INV_QTY',
            align: 'right',
            style: 'text-align:left',
            width: 80
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80
        },
        {
            header: "",
            flex: 1
        }],
        listeners: {
            beforeselect: function (test, record, index, eOpts) {
                if (Number(record.data.INV_QTY) > 0) {
                    return true;
                }
                else {
                    return false;
                }
            },
            selectionchange: function (model, records) {
                //msglabel('訊息區:');
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
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'center',
                        layout: 'fit',
                        split: true,
                        collapsible: false,
                        border: false,
                        items: [T1Grid]
                    }
                    
                ]
            }
        ]
    });
    
    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();

    //#region CheckboxModel
    Ext.define('overrides.selection.CheckboxModel', {
        override: 'Ext.selection.CheckboxModel',

        getHeaderConfig: function () {
            var config = this.callParent();

            if (Ext.isFunction(this.selectable)) {
                config.selectable = this.selectable;
                config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                    if (this.selectable(record)) {
                        record.selectable = false;
                        return '';
                    }
                    record.selectable = true;
                    return this.defaultRenderer();
                };
                this.on('beforeselect', function (rowModel, record, index, eOpts) {
                    return !this.selectable(record);
                }, this);
            }
            return config;
        },

        updateHeaderState: function () {
            // check to see if all records are selected
            var me = this,
                store = me.store,
                storeCount = store.getCount(),
                views = me.views,
                hdSelectStatus = false,
                selectedCount = 0,
                selected, len, i, notSelectableRowsCount = 0;

            if (!store.isBufferedStore && storeCount > 0) {
                hdSelectStatus = true;
                store.each(function (record) {
                    if (!record.selectable) {
                        notSelectableRowsCount++;
                    }
                }, this);
                selected = me.selected;

                for (i = 0, len = selected.getCount(); i < len; ++i) {
                    if (store.indexOfId(selected.getAt(i).id) > -1) {
                        ++selectedCount;
                    }
                }
                hdSelectStatus = storeCount === selectedCount; // + notSelectableRowsCount;
            }

            if (views && views.length) {
                me.column.setHeaderStatus(hdSelectStatus);
            }
        }
    });
    //#endregion

});