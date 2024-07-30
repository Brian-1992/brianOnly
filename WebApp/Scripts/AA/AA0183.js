Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Name = '庫房主動撥發作業';
    var T1F1 = '';
    var T1F2 = '';
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1LastRec = null;
    var T1Rec = 0;

    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0183/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_towhcombo = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0183/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        //autoLoad: true
    });
    var mLabelWidth = 90;
    var mWidth = 250;
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: mLabelWidth,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0183/GetMmCodeCombo', //指定查詢的Controller路徑
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
            labelWidth: mLabelWidth,
            width: mWidth
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
                            xtype: 'combo',
                            fieldLabel: '出庫庫房',
                            name: 'P1',
                            id: 'P1',
                            store: st_frwhcombo,
                            queryMode: 'local',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            labelAlign: 'right',
                            allowBlank: false,
                            fieldCls: 'required',
                            labelWidth: mLabelWidth,
                            width: mWidth,
                            listeners: {
                                select: function () {
                                    Ext.getCmp('T1btn1').setDisabled(false);
                                }
                            }
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1btn1',
                            disabled: true, 
                            handler: function () {
                                var f = this.up('form').getForm();
                                if (f.findField('P1').getValue() == null || f.findField('P1').getValue() == '') {
                                    Ext.Msg.alert('提醒', '請挑選出庫庫房');
                                }
                                else {
                                    T1Load();
                                }
                                msglabel('訊息區:');
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
            { name: 'APPQTY', type: 'string' },
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
            url: '/api/AA0183/AllM',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
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
                id: 'T1update', itemId: 'T1update', text: '主動撥發', disabled: true, handler: function () {
                    if (T1Query.getForm().findField('P1').getValue() == '') {
                        Ext.Msg.alert('提醒', '請挑選出庫庫房');
                    }
                    else {
                        st_towhcombo.load();
                        showWin2();
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
                text: "<b><font color=red>核撥量</font></b>",
                dataIndex: 'APPQTY',
                style: 'text-align:left',
                width: 80,
                align: 'right',
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    //fieldCls: 'required',
                    //selectOnFocus: true,
                    //allowBlank: false,
                    minValue: 1,
                    listeners: {
                        /*specialkey: function (field, e) {
                            if (e.getKey() === e.UP || e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var rowIndex = editPlugin.context.rowIdx;
                                if (e.getKey() === e.UP) {
                                    rowIndex -= 1;
                                }
                                else {
                                    rowIndex += 1;
                                }
                                editPlugin.startEdit(rowIndex, editPlugin.context.colIdx);
                            }
                        }*/
                    }
                }
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {

                    },
                    edit: function (editor, context, eOpts) {
                        var r = context.record;
                        var rowIndex = context.rowIdx;
                        var sm = T1Grid.getSelectionModel();
                        var appqty = (context.field === 'APPQTY') ? context.value : r.get('APPQTY');
                        if (Number(appqty) > 0) {
                            sm.select(rowIndex);
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                        var r = context.record;
                        var rowIndex = context.rowIdx;
                        var invqty = (context.field === 'INV_QTY') ? context.value : r.get('INV_QTY');
                        if (Number(invqty) <= 0 && Number(context.value) > 0) {
                            Ext.MessageBox.alert('錯誤', '庫存量不足');
                            context.cancel = true;
                            context.record.data[context.field] = context.originalValue;
                        }
                    }
                }
            })
        ],
        listeners: {
            beforeedit: function (editor, e) {
                if (Number(e.record.get('INV_QTY')) > 0) {
                    return true;
                }
                else {
                    return false;
                }
            },
            beforeselect: function (test, record, index, eOpts) {
                if (Number(record.data.INV_QTY) > 0 && Number(record.data.APPQTY) > 0) {
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

    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'combo',
                fieldLabel: '入庫庫房',
                name: 'TOWH',
                id: 'TOWH',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                labelAlign: 'right',
                allowBlank: false,
                fieldCls: 'required',
                labelWidth: mLabelWidth,
                width: mWidth,
                listeners: {
                    select: function () {
                        Ext.getCmp('T2btn1').setDisabled(false);
                    }
                }
            }
        ],
        buttons: [{
            itemId: 'apply', id: 'T2btn1', text: '確定', disabled: true, handler: function () {
                submitApply();
            }
        },
        {
            itemId: 'cancel', text: '取消', handler: hideWin2
        }
        ]
    });


    var win2;
    var winActWidth2 = 300;
    var winActHeight2 = 200;
    if (!win2) {
        win2 = Ext.widget('window', {
            title: '主動撥發',
            closeAction: 'hide',
            width: winActWidth2,
            height: winActHeight2,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T2Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth2 > 0) ? winActWidth2 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight2 > 0) ? winActHeight2 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth2 = width;
                    winActHeight2 = height;
                }
            }
        });
    }
    function showWin2() {
        if (win2.hidden) {
            win2.show();
        }
    }
    function hideWin2() {
        if (!win2.hidden) {
            win2.hide();
        }
    }

    function submitApply() {
        var selection = T1Grid.getSelection();
        if (selection.length === 0) {
            Ext.Msg.alert('提醒', '請勾選項目');
        }
        else {
            let name = '';
            let mmcode = '';
            let appqty = '';
            $.map(selection, function (item, key) {
                name += '「' + item.get('MMCODE') + '」<br>';
                mmcode += item.get('MMCODE') + ',';
                appqty += item.get('APPQTY') + ',';
            })
            var wh_no =  T2Form.getForm().findField("TOWH").getValue();
            Ext.MessageBox.confirm('庫存轉入藥局', '是否確定庫存轉入' + wh_no + '？院內碼如下<br>' + name,
                function (btn, text) {
                    if (btn === 'yes') {
                        myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0183/Apply',
                        method: reqVal_p,
                        params: {
                            MMCODE: mmcode,
                            APPQTY: appqty,
                            FRWH: T1Query.getForm().findField('P1').getValue(),
                            TOWH: T2Form.getForm().findField("TOWH").getValue()
                        },
                        //async: true,
                        success: function (response) {
                            myMask.hide();
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T1Grid.getSelectionModel().deselectAll();
                                T1Load();
                                msglabel('訊息區:庫存轉入' + wh_no + '成功');
                                
                            }
                            else {
                                Ext.MessageBox.alert('錯誤', data.msg);
                            }
                                
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }
            );
        }
        
        hideWin2();
        T1Load();
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