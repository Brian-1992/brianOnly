Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.MMCodeCombo']);

Ext.onReady(function () {
    var T1Create = '/api/AB0028/CreateM';
    var T1Update = '/api/AB0028/UpdateM';
    var T1Delete = '/api/AB0028/DeleteM';
    var T2Create = '/api/AB0028/CreateD';
    var T2Update = '/api/AB0028/UpdateD';
    var T2Delete = '/api/AB0028/DeleteD';
    var TDeleteMbyNoD = '/api/AB0028/DeleteMbyNoDetail';
    var T1Set = '';
    var T1LastRec = null;
    var T2LastRec = null;
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var DOCNO = '';
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APP_NAME', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'FLOWID_N', type: 'string' },
            { name: 'LIST_ID', type: 'string' }
        ]
    });
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'APPQTY', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0028/AllM',
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
                    p1: Ext.util.Format.date(T1Query.getForm().findField('D1').getValue(), 'Ymd'),
                    p2: Ext.util.Format.date(T1Query.getForm().findField('D2').getValue(), 'Ymd'),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var towh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AB0028/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    //var frwh_store = Ext.create('Ext.data.Store', {
    //    proxy: {
    //        type: 'ajax',
    //        actionMethods: {
    //            read: 'POST' // by default GET
    //        },
    //        url: '/api/AB0028/GetFrwhCombo',
    //        reader: {
    //            type: 'json',
    //            rootProperty: 'etts'
    //        }
    //    }, autoLoad: true
    //});
    var qWh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AB0028/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        },
        autoLoad: true
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0028/AllD',
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
                    p0: DOCNO//T1LastRec.data.DOCNO
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            },
            callback: function () {
                if (T1LastRec != null) {
                    if (T1LastRec.get('DOCNO') != '*' & T1LastRec.get('FLOWID') == "0201" & T1LastRec.get('APPID') == userId) {
                        var recNew = Ext.create('T2Model');
                        recNew.data.DOCNO = '*';
                        T2Store.add(recNew);
                        T2LastRec = recNew;

                        if (T2Store.getCount() == 1)
                            T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);//???? 3->2
                        else
                            T2Grid.editingPlugin.startEdit(0, 6);
                        Ext.getCmp('btnDel2').setDisabled(false);
                    } else Ext.getCmp('btnDel2').setDisabled(true);
                }
            }
        });
        //if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
        //    T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
        //    T2Store.load({
        //        params: {
        //            start: 0
        //        },
        //        callback: function () {
        //            if (T1LastRec.get('DOCNO') != '*' & T1LastRec.get('FLOWID') == "0201" & T1LastRec.get('APPID') == userId) {
        //                var recNew = Ext.create('T2Model');
        //                recNew.data.DOCNO = '*';
        //                T2Store.add(recNew);
        //                T2LastRec = recNew;

        //                if (T2Store.getCount() == 1)
        //                    T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);
        //                else
        //                    T2Grid.editingPlugin.startEdit(0, 6);
        //                Ext.getCmp('btnDel2').setDisabled(false);
        //            } else Ext.getCmp('btnDel2').setDisabled(true);
        //        }
        //    });
        //} else {
        //    DOCNO = '';
        //    T2Store.load();
        //    //T2Store.removeAll();
        //}
    }

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'textfield',
                id: 'P0',
                fieldLabel: '申請單號'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D1',
                id: 'D1',
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'D2',
                id: 'D2'
            }, {
                xtype: 'textfield',
                id: 'P4',
                fieldLabel: '申請人員',
                labelAlign: 'right'
            }, {
                xtype: 'combo',
                fieldLabel: '調出庫房',
                id: 'P3',
                queryMode: 'local',
                store: qWh_store,
                displayField: 'TEXT',
                valueField: 'VALUE',
                matchFieldWidth: false,
                listConfig: {
                    width: 230
                }
            }, {
                xtype: 'combo',
                fieldLabel: '調入庫房',
                id: 'P6',
                queryMode: 'local',
                matchFieldWidth: false,
                listConfig: {
                    width: 230
                },
                store: qWh_store,
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'checkboxgroup',
                //rowspan: 2,
                columns: 2,
                id: 'P7',
                items: [
                    { boxLabel: '申請中', name: 'P7', inputValue: '0201', checked: true },
                    { boxLabel: '調出中', name: 'P7', inputValue: '0202' },
                    { boxLabel: '調入中', name: 'P7', inputValue: '0203' },
                    { boxLabel: '已結案', name: 'P7', inputValue: '0299' }
                ],
                getValue: function () {
                    var val = [];
                    this.items.each(function (item) {
                        if (item.checked)
                            val.push(item.inputValue);
                    });
                    return val.toString();
                }
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
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
            }
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        listeners: {
            change: function () {
                T1LastRec = null;
                DOCNO = '';
                var recNew = Ext.create('T1Model');
                recNew.data.DOCNO = '*';
                recNew.data.FLOWID = "0201";
                recNew.data.APPID = userId;
                recNew.data.APP_NAME = userName;
                recNew.data.APPDEPT = userInidName;
                recNew.data.LIST_ID = '';
                T1Store.add(recNew);
                // ie-error 
                setTimeout(() => T1Grid.editingPlugin.startEdit(T1Store.indexOf(recNew), 7), 1000);
                T2Load();
            }
        },
        buttons: [
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                disabled: true,
                handler: function () {
                    var rows = T1Grid.getSelection();
                    if (rows.length >= 1) {
                        let name = '';
                        let docno = '';
                        for (i = 0; i < rows.length; i++) {
                            if (rows[i].data.DOCNO != '*') {
                                name += '「' + rows[i].data.DOCNO + '」<br>';
                                docno += rows[i].data.DOCNO + ',';
                            }
                        }
                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: T1Delete,
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnApply').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }, {
                text: '提出申請',
                id: 'btnApply',
                name: 'btnApply',
                disabled: true,
                handler: function () {
                    var rows = T1Grid.getSelection();
                    if (rows.length >= 1) {
                        let name = '';
                        let docno = '';
                        for (i = 0; i < rows.length; i++) {
                            if (rows[i].data.DOCNO != '*') {
                                name += '「' + rows[i].data.DOCNO + '」<br>';
                                docno += rows[i].data.DOCNO + ',';
                            }
                        }
                        Ext.MessageBox.confirm('訊息', '確認是否提出申請?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0028/UpdateFLOWID0202',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnApply').setDisabled(true);
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            }
        ]
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
                items: [T1Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID_N',
                width: 100
            },
            {
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 100
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT',
                width: 150
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "<span style='color:red'>調出庫房</span>",
                dataIndex: 'FRWH_N',
                width: 200,
                editor: {
                    xtype: 'combo',
                    store: towh_store,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldCls: 'required',
                    allowBlank: false,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx + 1);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>調入庫房</span>",
                dataIndex: 'TOWH_N',
                width: 200,
                editor: {
                    xtype: 'combo',
                    store: towh_store,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldCls: 'required',
                    allowBlank: false,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, editPlugin.context.colIdx - 1);
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
            select: function (model, record, index) {
                var t1Disabled = (model.selected.length == 1 && record.get('DOCNO') == '*');
                //Ext.getCmp('btnDel').setDisabled(t1Disabled);
                //Ext.getCmp('btnApply').setDisabled(t1Disabled);
                if (t1Disabled) {
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnApply').setDisabled(true);
                } else {
                    if (record.get('APPID') == userId & record.get('FLOWID') == "0201") {//自己申請才可修改
                        Ext.getCmp('btnDel').setDisabled(false);
                        Ext.getCmp('btnApply').setDisabled(false);
                    } else {
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnApply').setDisabled(true);
                    }
                }
            },
            deselect: function (model, record, index) {
                var t1Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('DOCNO') == '*');
                Ext.getCmp('btnDel').setDisabled(t1Disabled);
                Ext.getCmp('btnApply').setDisabled(t1Disabled);
            },
            selectionchange: function (model, records) {
                T1LastRec = records[0];
                if (T1LastRec == undefined) DOCNO = '';
                else DOCNO = T1LastRec.data.DOCNO;
                T2Load();
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
                    beforeedit: function (editor, context, eOpts) {
                        if (T1LastRec != null) {
                            if (T1LastRec.data.FLOWID == '0201' & T1LastRec.data.APPID == userId) {
                                //recNew.data.LIST_ID = '';
                                if (context.field == "FRWH_N") {
                                    if (T1LastRec.data.LIST_ID == "") return true;
                                    else return false;
                                } else return true;
                            } else return false;
                        }
                        return true;
                    },
                    validateedit: function (editor, context, eOpts) {
                    },
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var docno = r.get('DOCNO');
                        var towh = (context.field === 'TOWH_N') ? context.value : r.get('TOWH');
                        var frwh = (context.field === 'FRWH_N') ? context.value : r.get('FRWH');
                        switch (context.field) {
                            case 'TOWH_N':
                                r.set('TOWH', context.value);
                                var cr = towh_store.findRecord('VALUE', towh);
                                r.set('TOWH_N', cr.get('TEXT'));
                                break;
                            case 'FRWH_N':
                                r.set('FRWH', context.value);
                                var cr = towh_store.findRecord('VALUE', frwh);
                                r.set('FRWH_N', cr.get('TEXT'));
                                break;
                        }

                        if (docno == '*') //新增
                        {
                            if (towh == '' || frwh == '') return;

                            Ext.each(modifiedRec, function (item) {

                                if (item.crudState === 'C') {
                                    Ext.Ajax.request({
                                        url: T1Create,
                                        method: reqVal_p,
                                        params: {
                                            FRWH: frwh,
                                            TOWH: towh
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                var dbnr = data.etts[0];
                                                item.set('DOCNO', dbnr.DOCNO);
                                                item.set('FLOWID', dbnr.FLOWID);
                                                item.set('APPID', dbnr.APPID);
                                                item.set('FLOWID_N', dbnr.FLOWID_N);
                                                item.set('APP_NAME', dbnr.APP_NAME);
                                                item.set('APPTIME', dbnr.APPTIME);

                                                item.commit();

                                                var recNew = Ext.create('T1Model');
                                                recNew.data.DOCNO = '*';
                                                recNew.data.APPID = userId;
                                                recNew.data.APP_NAME = userName;
                                                recNew.data.FLOWID = "0201";
                                                recNew.data.APPDEPT = userInidName;
                                                recNew.data.LIST_ID = '';
                                                T1Store.add(recNew);

                                                var recNew2 = Ext.create('T2Model');
                                                recNew2.data.DOCNO = '*';
                                                T2Store.add(recNew2);
                                                msglabel('資料新增成功');

                                                T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew2), 3);
                                                T2LastRec = recNew2;
                                                Ext.getCmp('btnDel').setDisabled(false);
                                                Ext.getCmp('btnApply').setDisabled(false);
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
                            });
                        }
                        else {
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'U') {
                                    Ext.Ajax.request({
                                        url: T1Update,
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: docno,
                                            FRWH: frwh,
                                            TOWH: towh
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                item.commit();
                                                msglabel('資料更新成功');
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
        plain: true,
        buttons: [
            {
                text: '刪除',
                id: 'btnDel2',
                name: 'btnDel2',
                disabled: true,
                handler: function () {
                    var rows = T2Grid.getSelection();
                    if (rows.length >= 1) {
                        let docno = '';
                        let seq = '';
                        for (i = 0; i < rows.length; i++) {
                            docno += rows[i].data.DOCNO + ',';
                            seq += rows[i].data.SEQ + ',';
                        }
                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: T2Delete,
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('資料刪除成功');
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            Ext.getCmp('btnDel2').setDisabled(true);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
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
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            }, {
                text: "<span style='color:red'>院內碼</span>",
                dataIndex: 'MMCODE',
                width: 100,
                editor: {
                    xtype: 'mmcodecombo',
                    name: 'MMCODE',
                    fieldCls: 'required',
                    selectOnFocus: true,
                    allowBlank: false,
                    labelAlign: 'right',
                    msgTarget: 'side',
                    width: 300,
                    limit: 10,
                    queryUrl: '/api/AB0028/GetMmcodeCombo',
                    extraFields: ['BASE_UNIT'],
                    getDefaultParams: function () {
                        return { p1: T1LastRec.get('FRWH') };
                    },
                    listeners: {
                        select: function (c, r, i, e) {
                            T2LastRec.set('MMNAME_C', r.get('MMNAME_C'));
                            T2LastRec.set('MMNAME_E', r.get('MMNAME_E'));
                            T2LastRec.set('BASE_UNIT', r.get('BASE_UNIT'));
                        },
                        specialkey: function (field, e) {
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }
                        },
                        change: function (c, r, i, e) {
                            if (r.length == 8) {
                                
                                msglabel('');
                                Ext.Ajax.request({
                                    url: '/api/AB0028/CheckMmcodeValid',
                                    method: reqVal_p,
                                    params: {
                                        mmcode: r,
                                        wh_no: T1LastRec.data.FRWH
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        
                                        if (data.msg == "N") {
                                            
                                            msglabel('院內碼已停用，請重新選擇');
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    },
                                    //jsonData: data
                                });
                            }
                        }
                    }
                }
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                flex: 0.5
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                flex: 0.5
            },
            {
                text: "<span style='color:red'>申請數量</span>",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                editor: {
                    fieldCls: 'required',
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    allowBlank: false,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 3);
                            }

                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 6);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 6);
                            }
                        }
                    }
                }
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            }
        ],
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                if (T1LastRec != null) {
                    if ((T1LastRec.data.APPID == userId) & (T1LastRec.data.FLOWID == "0201")) return true;
                    else return false;
                }
                return false;
            },
            select: function (model, record, index) {
                var t2Disabled = (model.selected.length == 1 && record.get('DOCNO') == '*');
                if (t2Disabled)
                    Ext.getCmp('btnDel2').setDisabled(t2Disabled);
                else {
                    if ((T1LastRec.data.APPID == userId) & (T1LastRec.data.FLOWID == "0201")) {//自己申請才可修改
                        Ext.getCmp('btnDel2').setDisabled(false);
                    } else {
                        Ext.getCmp('btnDel2').setDisabled(true);
                    }
                }
            },
            deselect: function (model, record, index) {
                var t2Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('DOCNO') == '*');;
                Ext.getCmp('btnDel2').setDisabled(t2Disabled);
            },
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
                        var docno = r.get('DOCNO');
                        var mmcode = (context.field === 'MMCODE') ? context.value : r.get('MMCODE');
                        var appqty = (context.field === 'APPQTY') ? context.value : r.get('APPQTY');

                        if (docno == '*') //新增
                        {
                            if (mmcode == '' || appqty == '') return false;
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'C') {
                                    Ext.Ajax.request({
                                        url: T2Create,
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: T1LastRec.get('DOCNO'),
                                            MMCODE: mmcode,
                                            APPQTY: appqty
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                var dbnr = data.etts[0];
                                                item.set('DOCNO', dbnr.DOCNO);
                                                item.set('SEQ', dbnr.SEQ);
                                                item.commit();
                                                Ext.getCmp('btnDel2').setDisabled(false);
                                                var recNew = Ext.create('T2Model');
                                                recNew.data.DOCNO = '*';
                                                T2Store.add(recNew);
                                                var exist = T1Store.find('DOCNO', dbnr.DOCNO);
                                                if (exist >= 0) {
                                                    if (T1Store.getAt(exist).get('LIST_ID') == "")
                                                        T1Store.getAt(exist).set('LIST_ID', 'Y');
                                                }

                                                msglabel('資料新增成功');

                                                editor.startEdit(T2Store.indexOf(recNew), 3);
                                                T2LastRec = recNew;
                                            }
                                            else {
                                                item.reject();
                                                Ext.Msg.alert('提醒', data.msg, function () {
                                                    editor.startEdit(context.rowIdx, 3);
                                                });
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        },
                                        //jsonData: data
                                    });
                                }
                            });
                        }
                        else {
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'U') {
                                    Ext.Ajax.request({
                                        url: T2Update,
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: T1LastRec.get('DOCNO'),
                                            SEQ: r.get('SEQ'),
                                            MMCODE: mmcode,
                                            APPQTY: appqty
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
                                }
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
    function DeleteMbyNoD() {
        Ext.Ajax.request({
            url: TDeleteMbyNoD,
            method: reqVal_p,
            success: function (response) {
                T1Load();
            }, failure: function (response) {
                T1Load();
            },
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
                        height: '40%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '60%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Query]
        }
        ]
    });
    //DeleteMbyNoD();
});