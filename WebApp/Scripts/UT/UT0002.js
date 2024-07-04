Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.onReady(function () {
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var docno = Ext.getUrlParam('docno');
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCTYPE', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'EXP_STATUS', type: 'string' },
            { name: 'SEQ', type: 'int' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'QTY', type: 'string' },
            { name: 'EDIT_TYPE', type: 'string' },
            { name: 'IS_CLOSE', type: 'string' },
            { name: 'IS_ADD', type: 'string' }
        ]
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'SEQ', type: 'int' },
            { name: 'MMCODE', type: 'string' },
            { name: 'EXP_DATE', type: 'date' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'APVQTY', type: 'string' }
        ]
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'displayfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [{
            name: 'P0',
            fieldLabel: '單據號碼',
            labelAlign: 'right'
        }]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        autoLoad: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/UT0002/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: docno
                };
                Ext.apply(store.proxy.extraParams, np);
            }, load: function (store) {
                T1Query.getForm().findField('P0').setValue(docno);
                if (store.data.length == 0) {
                    T1Tool.down('#return').setVisible(false);
                    Ext.Msg.alert('效期', '無效期資料');
                } else {
                    if (store.data.items[0].data.IS_CLOSE == "Y")
                        T1Tool.down('#return').setVisible(false);
                    if (store.data.items[0].data.IS_ADD == "N")
                        T2Tool.setVisible(false);
                }
            }
        }
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'EXP_DATE', direction: 'ASC' }, { property: 'LOT_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/UT0002/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var seq = '';
                var doctype = '';
                var mflowid = '';
                if (T1LastRec != null) {
                    seq = T1LastRec.data.SEQ;
                    doctype = T1LastRec.data.DOCTYPE; 
                    mflowid = T1LastRec.data.FLOWID;
                }
                var np = {
                    flowid: mflowid,
                    p0: docno,
                    p1: seq,
                    doctype:doctype
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            change: function (T1Tool, pageData) {
                T1LastRec = null;
                T2Store.removeAll();
            }
        },
        buttons: [{
            itemId: 'return', text: '退回修正效期', handler: function () {
                Ext.MessageBox.confirm('退回修正效期', '是否確定退回修正效期?', function (btn, text) {
                    if (btn == 'yes') {
                        if (checkData())
                            T1Submit();
                    }
                });
            }
        }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            success: function (form, action) {
                myMask.hide();
                T1Store.load();
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('失敗', action.result.msg);
                        break;
                }
            }
        });
    }

    Ext.define('overrides.selection.CheckboxModel', {
        override: 'Ext.selection.CheckboxModel',

        getHeaderConfig: function () {
            var config = this.callParent();
            if (Ext.isFunction(this.selectable)) {
                config.selectable = this.selectable;
                config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                    if (this.selectable(record)) {
                        record.selectable = true;
                        return '';
                    }
                    record.selectable = true;
                    return this.defaultRenderer();
                };
                //this.on('beforeselect', function (rowModel, record, index, eOpts) {
                //    return !this.selectable(record);
                //}, this);
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
                hdSelectStatus = storeCount == selectedCount + notSelectableRowsCount;
            }

            if (views && views.length) {
                me.column.setHeaderStatus(hdSelectStatus);
            }
        }
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
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
        },{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 0,
            mode: 'MULTI',
            selType: 'checkboxmodel',
            selectable: function (record) {
                return record.get('EXP_STATUS') == "R";
            }
        },
        
        columns: {
            //defaults: {
            //    renderer: function (value, metaData, record) {
            //        if (record.get('EXP_STATUS') === 'R') {
            //            metaData.tdAttr = 'style="background:#EBF49D;"';
            //        }
            //        return value;
            //    }
            //},
            items: [{
                xtype: 'rownumberer'
            }, {
                text: "調出庫房",
                dataIndex: 'FRWH_N',
                width: 140
            }, {
                text: "調入庫房",
                dataIndex: 'TOWH_N',
                width: 140
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data.EXP_STATUS == "R") {
                        return "<span style='color:red'>" + val + "</span>";
                    }
                    return val;
                }
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                flex: 0.5,
                sortable: true
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                flex: 0.5,
                sortable: true
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 100
            }, {
                text: "調入數量",
                //xtype: 'numbercolumn',若指定,則小數也要指定位數 format: '0,000.000'
                format: '0,000',
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'QTY',
                width: 80
            }]
        },
        listeners: {
            beforeselect: function (row, model, index) {
                T1LastRec = model;
                T2Store.load();
                if (model.data.EXP_STATUS == "R")
                    return false;
            }
        }
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        url: '/api/UT0002/UpdateStatus',
        items: [{
            xtype: 'hidden',
            name: 'SEQ_LIST'
        }, {
            xtype: 'hidden',
            name: 'DOCNO'
        }
        ]
    });

    function checkData() {
        var f = T1Form.getForm();
        var rows = T1Grid.getSelection();

        if (rows.length == 0) {
            Ext.MessageBox.alert('提醒', '請至少勾選一筆資料');
            return false;
        } else {
            var seq_list = '';
            for (i = 0; i < rows.length; i++) {
                seq_list += rows[i].data.SEQ + ',';
            }
            f.findField("DOCNO").setValue(docno);
            f.findField("SEQ_LIST").setValue(seq_list.substr(0, seq_list.length - 1));
        }
        return true;
    }

    function insertRow() {
        var recNew = Ext.create('T2Model');
        recNew.data.MMCODE = T1LastRec.data.MMCODE;
        T2Store.add(recNew);
    }

    function checkData2() {
        var total = 0;
        var sum = 0;
        var exp_date = '';
        var lot_no = '';
        var move_qty = '';
        var tmp_date = '';
        
        for (var i = 0; i < T2Store.data.length; i++) {
            tmp_date = Ext.Date.format(T2Store.data.items[i].data.EXP_DATE, 'Xmd');
            if (T2Store.data.items[i].data.APVQTY != "" & T2Store.data.items[i].data.APVQTY != "0") {
                total = total + parseFloat(T2Store.data.items[i].data.APVQTY);
                exp_date += Ext.Date.format(T2Store.data.items[i].data.EXP_DATE, 'Y/m/d') + ',';
                lot_no += T2Store.data.items[i].data.LOT_NO + ',';
                move_qty += T2Store.data.items[i].data.APVQTY + ',';
            }
        }
        
        if (total != T1LastRec.data.QTY) {
            Ext.MessageBox.alert('提醒', '數量合計' + total.toString() + '與轉入數量' + T1LastRec.data.QTY + '不相同!');
            return false;
        } else if (exp_date != "") {
            var f = T2Form.getForm();
            f.findField("DOCNO").setValue(docno);
            f.findField("DOCTYPE").setValue(T1LastRec.data.DOCTYPE);
            f.findField("SEQ").setValue(T1LastRec.data.SEQ);
            f.findField("MMCODE").setValue(T1LastRec.data.MMCODE);
            f.findField("EXP_DATE_LIST").setValue(exp_date.substr(0, exp_date.length - 1));
            f.findField("LOT_NO_LIST").setValue(lot_no.substr(0, lot_no.length - 1));
            f.findField("APVQTY_LIST").setValue(move_qty.substr(0, move_qty.length - 1));
        }
        return true;
    }

    var T2Form = Ext.widget({
        xtype: 'form',
        url: '/api/UT0001/CreateD',
        items: [{
            xtype: 'hidden',
            name: 'LOT_NO_LIST'
        }, {
            xtype: 'hidden',
            name: 'EXP_DATE_LIST'
        }, {
            xtype: 'hidden',
            name: 'APVQTY_LIST'
        }, {
            xtype: 'hidden',
            name: 'MMCODE'
        }, {
            xtype: 'hidden',
            name: 'DOCNO'
        }, {
            xtype: 'hidden',
            name: 'SEQ'
        }, {
            xtype: 'hidden',
            name: 'DOCTYPE'
        }
        ]
    });

    function T2Submit() {
        var f = T2Form.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            success: function (form, action) {
                myMask.hide();
                T2Store.load();
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('失敗', action.result.msg);
                        break;
                }
            }
        });
    }
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'new', text: '新增', handler: function () {
                insertRow();
            }
        }, {
                itemId: 'save', text: '儲存', handler: function () {
                if (checkData2())
                    T2Submit();
            }
        }
        ]
    });
    var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1,
        autoCancel: false
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [
            cellEditing
        ], listeners: {
            beforeedit: function (e, editor) { //cellEditor, context, eOpts ==> context.rowIdx;
                if (T1LastRec.data.IS_ADD == 'Y') return true;
                else return false;
            }
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
             items: [T2Tool],
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            sortable: false,
            width: 100
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            sortable: false,
            xtype: 'datecolumn',
            format: 'Xmd',
            width: 100,
            editor: {
                xtype: 'datefield',
                allowBlank: false
            }
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            sortable: false,
            width: 140,
            editor: {
                xtype: 'textfield',
                allowBlank: false
            }
        }, {
            text: "數量",
            dataIndex: 'APVQTY',
            align: 'right',
            format: '0,000',
            style: 'text-align:left',
            sortable: false,
            width: 100,
            editor: {
                xtype: 'textfield',
                allowBlank: false,
                regex: /^\d+(\.\d+)?$/
            }
        }
        ]
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
            region: 'north',
            layout: 'fit',
            collapsible: false,
            height: '60%',
            border: false,
            items: [T1Grid]
        }, {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T2Grid]
        }]
    });
});