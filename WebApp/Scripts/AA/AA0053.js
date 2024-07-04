Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.MMCodeCombo']);

Ext.onReady(function () {
    var T1Create = '/api/AA0053/Create';
    var T1Delete = '/api/AA0053/DeleteM';
    var T2Update = '/api/AA0053/UpdateD';
    var T2Delete = '/api/AA0053/Delete';
    var T1Set = '';
    var T1LastRec = null;
    var T2LastRec = null;
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var T1F1 = '';
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' }
        ]
    });
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'PACK_UNIT0', type: 'string' },
            { name: 'PACK_QTY0', type: 'string' },
            { name: 'PACK_UNIT1', type: 'string' },
            { name: 'PACK_QTY1', type: 'string' },
            { name: 'PACK_UNIT2', type: 'string' },
            { name: 'PACK_QTY2', type: 'string' },
            { name: 'PACK_UNIT3', type: 'string' },
            { name: 'PACK_QTY3', type: 'string' },
            { name: 'PACK_UNIT4', type: 'string' },
            { name: 'PACK_QTY4', type: 'string' },
            { name: 'PACK_UNIT5', type: 'string' },
            { name: 'PACK_QTY5', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0053/All',
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
        T1Tool.moveFirst();
    }

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 5, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0053/AllD',
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
                    p0: T1F1
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
            //,
            //callback: function () {
            //    if (T1LastRec != null) {
            //        if (T1LastRec.get('WH_NO') != '*') {
            //            var recNew = Ext.create('T2Model');
            //            recNew.data.WH_NO = '*';
            //            T2Store.add(recNew);
            //            T2LastRec = recNew;

            //            //if (T2Store.getCount() == 1)
            //                //T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);//???? 3->2
            //            //else
            //                //T2Grid.editingPlugin.startEdit(0, 6);
            //            //Ext.getCmp('btnDel2').setDisabled(false);
            //        } else {
            //            //Ext.getCmp('btnDel2').setDisabled(true);
            //        }
            //    }
            //}
        });
    }

    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0053/GetMMCodeCombo', //指定查詢的Controller路徑
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: 'PH1S'
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'textfield',
                id: 'PP',
                fieldLabel: '庫別代碼',
                labelAlign: 'right',
                width: 150,
                readOnly: true
            },
            T1QueryMMCode,
            {
                xtype: 'button',
                text: '查詢',
                handler: function () {
                    T1Load();
                    viewport.down('#form').collapse();
                }
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P0').focus();
                    msglabel('訊息區:');
                    T1Query.getForm().findField('PP').setValue("PH1S");
                }
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
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
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            //{
            //    text: "",
            //    dataIndex: 'WH_NO',
            //    width: 20
            //},
            {
                text: "<span style='color:red'>院內碼</span>",
                dataIndex: 'MMCODE',
                width: 100,
                //editor: {
                //    xtype: 'mmcodecombo',
                //    name: 'MMCODE',
                //    fieldCls: 'required',
                //    selectOnFocus: true,
                //    allowBlank: false,
                //    labelAlign: 'right',
                //    msgTarget: 'side',
                //    width: 300,
                //    limit: 10,
                //    queryUrl: '/api/AA0053/GetMmcodeCombo2',
                //    getDefaultParams: function () {
                //        return { p1: 'PHIS' };
                //    },
                //    listeners: {
                //        select: function (c, r, i, e) {
                //            T1LastRec.set('MMNAME_E', r.get('MMNAME_E'));
                //        }
                //        //,
                //        //specialkey: function (field, e) {
                //        //    if (e.getKey() === e.RIGHT) {
                //        //        var editPlugin = this.up().editingPlugin;
                //        //        editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                //        //    }
                //        //}
                //    }
                //}
            },
            {
                text: "品名",
                dataIndex: 'MMNAME_E',
                flex: 0.5
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            select: function (model, record, index) {
                var t1Disabled = (model.selected.length == 1 && record.get('WH_NO') == '*');
                //Ext.getCmp('btnDel').setDisabled(t1Disabled);
                //Ext.getCmp('btnApply').setDisabled(t1Disabled);
                //if (t1Disabled) {
                //    Ext.getCmp('btnDel').setDisabled(true);
                //    Ext.getCmp('btnApply').setDisabled(true);
                //} else {
                //    Ext.getCmp('btnDel').setDisabled(false);
                //    Ext.getCmp('btnApply').setDisabled(false);
                //}
            },
            deselect: function (model, record, index) {
                var t1Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('WH_NO') == '*');
                //Ext.getCmp('btnDel').setDisabled(t1Disabled);
                //Ext.getCmp('btnApply').setDisabled(t1Disabled);
            },
            selectionchange: function (model, records) {
                T1LastRec = records[0];
                if (T1LastRec == undefined) {
                    T1F1 = '';
                }
                else {
                    T1F1 = T1LastRec.data.MMCODE;
                    T2Load();
                }
                //if ((T1LastRec.get('APPID') == userId) && (T1LastRec.get('FLOWID') == "0201")) {//自己申請才可修改
                //   Ext.getCmp('btnDel2').setDisabled(false);
                //} else {
                // Ext.getCmp('btnDel2').setDisabled(true);
                //}
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var wh_no = r.get('WH_NO');
                        //var towh = (context.field === 'TOWH_N') ? context.value : r.get('TOWH');
                        var mmcode = (context.field === 'MMCODE_N') ? context.value : r.get('MMCODE');
                        //switch (context.field) {
                        //    case 'MMCODE_N':
                        //        r.set('MMCODE', context.value);
                        //        var cr = towh_store.findRecord('VALUE', mmcode);
                        //        r.set('MMCODE_N', cr.get('TEXT'));
                        //        break;
                        //}

                        if (wh_no == '*') //新增
                        {
                            Ext.Ajax.request({
                                url: T1Create,
                                method: reqVal_p,
                                params: {
                                    MMCODE: mmcode
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        var dbnr = data.etts[0];
                                        item.set('WH_NO', dbnr.WH_NO);
                                        item.set('MMCODE', dbnr.MMCODE);
                                        item.set('MMNAME_E', dbnr.MMNAME_E);

                                        item.commit();

                                        var recNew = Ext.create('T1Model');
                                        recNew.data.WH_NO = '*';
                                        recNew.data.MMCODE = '';
                                        recNew.data.MMNAME_E = '';
                                        T1Store.add(recNew);

                                        var recNew2 = Ext.create('T2Model');
                                        recNew2.data.WH_NO = '*';
                                        T2Store.add(recNew2);
                                        msglabel('資料新增成功');

                                        T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew2), 3);
                                        T2LastRec = recNew2;
                                        //Ext.getCmp('btnDel').setDisabled(false);
                                        //Ext.getCmp('btnApply').setDisabled(false);
                                    }
                                    else {
                                        Ext.Msg.alert('提醒', data.msg);
                                        item.reject();
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                },
                                //jsonData: data
                            });
                        }
                        
                    },
                    canceledit: function (editor, context, eOpts) {
                        //alert('onCancelEdit');
                    }
                }
            })
        ]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
        //,
        //buttons: [
        //    {
        //        text: '刪除',
        //        id: 'btnDel2',
        //        name: 'btnDel2',
        //        disabled: true,
        //        handler: function () {
        //            var rows = T2Grid.getSelection();
        //            if (rows.length >= 1) {
        //                let docno = '';
        //                let seq = '';
        //                for (i = 0; i < rows.length; i++) {
        //                    docno += rows[i].data.DOCNO + ',';
        //                    seq += rows[i].data.SEQ + ',';
        //                }
        //                Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
        //                    if (btn === 'yes') {
        //                        Ext.Ajax.request({
        //                            url: T2Delete,
        //                            method: reqVal_p,
        //                            params: {
        //                                DOCNO: docno,
        //                                SEQ: seq
        //                            },
        //                            success: function (response) {
        //                                var data = Ext.decode(response.responseText);
        //                                if (data.success) {
        //                                    msglabel('資料刪除成功');
        //                                    T2Grid.getSelectionModel().deselectAll();
        //                                    T2Load();
        //                                    Ext.getCmp('btnDel2').setDisabled(true);
        //                                }
        //                            },
        //                            failure: function (response) {
        //                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
        //                            }
        //                        });
        //                    }
        //                });
        //            }
        //        }
        //    }
        //]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "品名",
                dataIndex: 'MMNAME_E',
                width: 350
            },
            {
                text: "<span style='color:red'>原裝箱單位</span>", //3
                dataIndex: 'PACK_UNIT0',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 4);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>原裝箱數量</span>", //4
                dataIndex: 'PACK_QTY0',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 3);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 5);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領包裝1</span>",//5
                dataIndex: 'PACK_UNIT1',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 4);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領數量1</span>",//6
                dataIndex: 'PACK_QTY1',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 5);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 7);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領包裝2</span>",//7
                dataIndex: 'PACK_UNIT2',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 8);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領數量2</span>",//8
                dataIndex: 'PACK_QTY2',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 7);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 9);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領包裝3</span>",//9
                dataIndex: 'PACK_UNIT3',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 8);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 10);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領數量3</span>",//10
                dataIndex: 'PACK_QTY3',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 9);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 11);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領包裝4</span>",//11
                dataIndex: 'PACK_UNIT4',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 10);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 12);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領數量4</span>",//12
                dataIndex: 'PACK_QTY4',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 11);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 13);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領包裝5</span>",//13
                dataIndex: 'PACK_UNIT5',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 12);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 14);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>申領數量5</span>",//14
                dataIndex: 'PACK_QTY5',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 13);
                            }
                        }
                    }
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    validateedit: function (editor, context, eOpts) {
                    },
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var pack_unit0 = (context.field === 'PACK_UNIT0') ? context.value : r.get('PACK_UNIT0');
                        var pack_qty0 = (context.field === 'PACK_QTY0') ? context.value : r.get('PACK_QTY0');
                        var pack_unit1 = (context.field === 'PACK_UNIT1') ? context.value : r.get('PACK_UNIT1');
                        var pack_qty1 = (context.field === 'PACK_QTY1') ? context.value : r.get('PACK_QTY1');
                        var pack_unit2 = (context.field === 'PACK_UNIT2') ? context.value : r.get('PACK_UNIT2');
                        var pack_qty2 = (context.field === 'PACK_QTY2') ? context.value : r.get('PACK_QTY2');
                        var pack_unit3 = (context.field === 'PACK_UNIT3') ? context.value : r.get('PACK_UNIT3');
                        var pack_qty3 = (context.field === 'PACK_QTY3') ? context.value : r.get('PACK_QTY3');
                        var pack_unit4 = (context.field === 'PACK_UNIT4') ? context.value : r.get('PACK_UNIT4');
                        var pack_qty4 = (context.field === 'PACK_QTY4') ? context.value : r.get('PACK_QTY4');
                        var pack_unit5 = (context.field === 'PACK_UNIT5') ? context.value : r.get('PACK_UNIT5');
                        var pack_qty5 = (context.field === 'PACK_QTY5') ? context.value : r.get('PACK_QTY5');

                        Ext.Ajax.request({
                            url: T2Update,
                            method: reqVal_p,
                            params: {
                                MMCODE: T1F1,
                                PACK_UNIT0: pack_unit0,
                                PACK_QTY0: pack_qty0,
                                PACK_UNIT1: pack_unit1,
                                PACK_QTY1: pack_qty1,
                                PACK_UNIT2: pack_unit2,
                                PACK_QTY2: pack_qty2,
                                PACK_UNIT3: pack_unit3,
                                PACK_QTY3: pack_qty3,
                                PACK_UNIT4: pack_unit4,
                                PACK_QTY4: pack_qty4,
                                PACK_UNIT5: pack_unit5,
                                PACK_QTY5: pack_qty5
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    item.APPQTY = appqty;
                                    item.commit();
                                    msglabel('資料更新成功');

                                    //editor.startEdit(context.rowIdx + 1, 6);
                                }
                                else {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 6);
                                    });

                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });
                    },
                    canceledit: function (editor, context, eOpts) {
                        //alert('onCancelEdit');
                    }
                }
            })
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
            itemId: 't1Grid',
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
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        }
        ]
    });

    T1Query.getForm().findField('PP').setValue("PH1S");
});