
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
    var T2LastRec = null;
    //var isFirst = true;
    var suggest = 'N';
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
            { name: 'IS_CLOSE', type: 'string' }
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
            { name: 'INV_QTY', type: 'float' },
            { name: 'APVQTY', type: 'string' },
            { name: 'ISFOUND', type: 'string' }
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
        }, {
            xtype: 'splitter'
        }, {
            xtype: 'combo',
            fieldLabel: '效期管理狀態',
            name: 'P1',
            id: 'P1',
            width: 170,
            editable: false,
            store: [["", "全部"], ["R", "退回"]]
        }, {
            xtype: 'button',
            text: '查詢',
            iconCls: 'TRASearch',
            handler: T1Load
        }, {
            xtype: 'button',
            text: '清除',
            iconCls: 'TRAClear',
            handler: function () {
                var f = this.up('form').getForm();
                f.findField('P1').setValue("");
                f.findField('P1').focus();
            }
        }]
    });

    function T1Load() {
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        T1Tool.moveFirst();
    }

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
            url: '/api/UT0001/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: docno,
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }, load: function (store) {
                T1Query.getForm().findField('P0').setValue(docno);
                if (store.data.length == 0) {
                    T1Tool.down('#move').setDisabled(true);
                    //Ext.Msg.alert('效期', '無效期資料');
                } else {
                    if (store.data.items[0].data.IS_CLOSE == "Y") {
                        T1Tool.down('#move').setVisible(false);
                        T2Tool.down('#out').setVisible(false);
                        T2Tool.down('#save').setVisible(false);
                        T2Grid.getView().getHeaderAtIndex(5).setText("數量");
                    } else T2Grid.getView().getHeaderAtIndex(5).addCls('x-grid-row-summary');
                    if (store.data.items[0].data.IS_ADD == "Y") 
                        T2Tool.down('#new').setVisible(true);
                    T1Tool.down('#move').setDisabled(false);
                }
                //if (store.data.length >= 0) {
                //    if (isFirst) {
                //        isFirst = false;
                //        T1Grid.getView().select(0);
                //    }
                //}
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
            url: '/api/UT0001/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var wh_no = '';
                var seq = '';
                var doctype = '';
                var edit_type = '';
                var qty = '0';
                var mm_code = '';
                var mflowid = '';
                if (T1LastRec != null) {
                    wh_no = T1LastRec.data.FRWH;
                    seq = T1LastRec.data.SEQ;
                    mm_code = T1LastRec.data.MMCODE;
                    doctype = T1LastRec.data.DOCTYPE;
                    edit_type = T1LastRec.data.EDIT_TYPE;
                    qty = T1LastRec.data.QTY;
                    mflowid = T1LastRec.data.FLOWID;
                }
                var np = {
                    flowid: mflowid,
                    doctype: doctype,
                    edit_type: edit_type,
                    docno: docno,
                    p2: wh_no,
                    p3: seq,
                    p4: mm_code,
                    p5: suggest,
                    p6: qty
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });



    var T1Form = Ext.widget({
        xtype: 'form',
        url: '/api/UT0001/Move',
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
            itemId: 'move', text: '修正效期', disabled: true, handler: function () {
                Ext.MessageBox.confirm('修正效期', '是否確定修正效期?', function (btn, text) {
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
    function isEdit() {
        //switch (T1LastRec.data.DOCTYPE) {
        //    case "TR":
        //    case "TR1":
        //        if (T1LastRec.data.FLOWID == "0202" | (T1LastRec.data.FLOWID == "0203" & T1LastRec.data.EXP_STATUS == "R"))
        //            return true;
        //        else return false;
        //        break;
        //    case "SP":
        //        if (T1LastRec.data.FLOWID == "0501") return true;
        //        else return false;
        //        break;
        //    default:
        //        return false;
        //        break;
        //}
        if (T1LastRec.data.EDIT_TYPE == 1) return true;
        else return false;
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
        }, {
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
                return record.get('EXP_STATUS') == "";
            }
        },
        columns: [{
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
                //if (record.data.EXP_CNT == 0) {
                //    meta.tdCls = 'readOnly';
                //}
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
            text: "調出數量",
            dataIndex: 'QTY',
            style: 'text-align:left',
            align: 'right',
            format: '0,000',
            width: 80
        }],
        listeners: {
            beforeselect: function (row, model, index) {
                T1LastRec = model;
                if (T1LastRec != null) {
                    if (isEdit()) {
                        T2Grid.down('#out').setDisabled(false);
                        T2Grid.down('#new').setDisabled(false);
                        T2Grid.down('#save').setDisabled(false);
                        T2Grid.headerCt.items.getAt(4).show();
                    } else {
                        T2Grid.down('#out').setDisabled(true);
                        T2Grid.down('#new').setDisabled(true);
                        T2Grid.down('#save').setDisabled(true);
                        T2Grid.headerCt.items.getAt(4).hide();
                    }
                }
                suggest = 'N';
                T2Store.load();
                if (model.data.EXP_STATUS == "")
                    return false;
            }
        }
    });

    var rEditor = Ext.create('Ext.grid.plugin.RowEditing', {
        clicksToEdit: 2,
        listeners: {
            edit: function (editor, e) {
                // 1 = 1;
            }
        }
    });

    function insertRow() {
        var recNew = Ext.create('T2Model');
        recNew.data.ISFOUND = 'N';
        recNew.data.MMCODE = T1LastRec.data.MMCODE;
        T2Store.add(recNew);
        //rEditor.startEdit(1, 2);
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
                //2019/8/26 庫存可為負數
                if (T1LastRec.data.IS_ADD=="N"){
                    if (parseFloat(T2Store.data.items[i].data.APVQTY) > parseFloat(T2Store.data.items[i].data.INV_QTY)) {
                        Ext.MessageBox.alert('提醒', tmp_date + '效期' + T2Store.data.items[i].data.LOT_NO + '批號移出數量(' + T2Store.data.items[i].data.APVQTY + ')不可大於庫存數量(' + T2Store.data.items[i].data.INV_QTY + ')!');
                        return false;
                        }
                }
                total = total + parseFloat(T2Store.data.items[i].data.APVQTY);
                exp_date += Ext.Date.format(T2Store.data.items[i].data.EXP_DATE, 'Y/m/d') + ',';
                lot_no += T2Store.data.items[i].data.LOT_NO + ',';
                move_qty += T2Store.data.items[i].data.APVQTY + ',';
            }
        }
        if (total != T1LastRec.data.QTY) {
            Ext.MessageBox.alert('提醒', '移出數量合計' + total.toString() + '與轉出數量' + T1LastRec.data.QTY + '不相同!');
            return false;
        } else if (exp_date != "") {
            var f = T2Form.getForm();
            f.findField("DOCNO").setValue(docno);
            f.findField("DOCTYPE").setValue(T1LastRec.data.DOCTYPE);
            f.findField("SEQ").setValue(T1LastRec.data.SEQ);
            f.findField("MMCODE").setValue(T1LastRec.data.MMCODE);
            f.findField("WH_NO").setValue(T1LastRec.data.FRWH);
            f.findField("EXP_DATE_LIST").setValue(exp_date.substr(0, exp_date.length - 1));
            f.findField("LOT_NO_LIST").setValue(lot_no.substr(0, lot_no.length - 1));
            f.findField("APVQTY_LIST").setValue(move_qty.substr(0, move_qty.length - 1));
        }
        return true;
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'out', text: '先進先出', disabled: true, handler: function () {
                suggest = 'Y';
                T2Store.load();
            }
        }, {
                itemId: 'new', text: '新增', disabled: true, hidden: true, handler: function () {
                insertRow();
            }
        }, {
            itemId: 'save', text: '儲存', disabled: true, handler: function () {
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
        ],
        listeners: {
            beforeselect: function (row, model, index) {
                T2LastRec = model;
            },
            beforeedit: function (e, editor) { //cellEditor, context, eOpts ==> context.rowIdx;
                if (T2LastRec.data.ISFOUND == 'N') return true;
                else if (editor.column.dataIndex == "LOT_NO" | editor.column.dataIndex == "EXP_DATE") return false;
                return isEdit();
            }
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool],
        }
        ],
        //selModel: {
        //    checkOnly: true,
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
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
            text: "庫存數量",
            dataIndex: 'INV_QTY',
            align: 'right',
            format: '0,000',
            style: 'text-align:left',
            sortable: false,
            width: 100
        }, {
            text: '移出數量',
            dataIndex: 'APVQTY',
            align: 'right',
            style: 'text-align:left',
            width: 100,
            tdCls: 'input',
            sortable: false,
            editor: {
                xtype: 'textfield',
                allowBlank: false,
                regex: /^\d+(\.\d+)?$/
            }
        }
        ]
    });

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
            name: 'WH_NO'
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
                suggest = 'N';
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