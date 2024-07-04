Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.MMCodeCombo']);

Ext.onReady(function () {
    var T1Create = '/api/AB0010/MasterCreate';
    var T1Update = '/api/AB0010/MasterUpdate';
    var T1Delete = '/api/AB0010/MasterDelete';
    var T2Create = '/api/AB0010/DetailCreate';
    var T2Update = '/api/AB0010/DetailUpdate';
    var T2Delete = '/api/AB0010/DetailDelete';
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '手動申請';
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];

    var viewModel = Ext.create('WEBAPP.store.AB.AB0010VM');

    var T1Store = viewModel.getStore('ME_DOCM');
    T1Store.on('load', function () {
        var recNew = Ext.create('WEBAPP.model.ME_DOCM');
        recNew.data.DOCNO = '*';
        recNew.data.APP_NAME = userId + ' ' + userName;
        recNew.data.APPDEPT_NAME = userInid + ' ' + userInidName;
        T1Store.add(recNew);
    });
    function T1Load() {
        //T1Store.getProxy().setExtraParam("field1", T1Query.getForm().findField('field1').getValue());
        //T1Store.getProxy().setExtraParam("field2", T1Query.getForm().findField('field2').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", userId);
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        //T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("DOCTYPE", 'MR');
        T1Store.getProxy().setExtraParam("FLOWID", '0101'); 
        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                },
                callback: function () {
                    if (T1LastRec.get('DOCNO') != '*')
                    {
                        var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                        recNew.data.DOCNO = '*';
                        recNew.data.SUGGEST_QTY = null;
                        T2Store.add(recNew);
                        T2LastRec = recNew;
                        
                        T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);
                    }
                }
            });
        }
    }

    var T3Store = viewModel.getStore('SUPPLY_INID');
    function T3Load() {
        //T3Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        T3Store.on('load', function () {
            //Ext.getCmp('FRWH').setValue('');
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: T3Store.getAt(0).get('WH_NO')
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    T4Load(data);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
                //jsonData: data
            });
        });
    }

    var T4Store = viewModel.getStore('FRWH');
    function T4Load(wh_grade) {
        T4Store.getProxy().setExtraParam("WH_GRADE", wh_grade);
       T4Store.load({
            params: {
                start: 0
            }
        });
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '單據號碼',
                        name: 'P1',
                        id: 'P1',
                        enforceMaxLength: true,
                        maxLength: 21,
                        labelWidth: 60,
                        width: 170,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Load();
                                }
                            }
                        }
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '申請庫房代碼',
                        name: 'P4',
                        id: 'P4',
                        enforceMaxLength: true,
                        maxLength: 6,
                        width: 170,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Load();
                                }
                            }
                        }
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '核撥庫房代碼',
                        name: 'P6',
                        id: 'P6',
                        enforceMaxLength: true,
                        maxLength: 8,
                        width: 170,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Load();
                                }
                            }
                        }
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T2Store.removeAll();
                            T1Grid.getSelectionModel().deselectAll();
                            T1Load();

                            Ext.getCmp('btnDel').setDisabled(true);
                            Ext.getCmp('btnCopy').setDisabled(true);
                            Ext.getCmp('btnSubmit').setDisabled(true);

                            Ext.getCmp('btnDel2').setDisabled(true);
                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');
                        }
                    }
                ]
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    if (item.get('DOCNO') === '*') return;
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            if (item.get('DOCNO') === '*') return;
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

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
                                            Ext.getCmp('btnCopy').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
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
            },
            {
                text: '複製',
                id: 'btnCopy',
                name: 'btnCopy',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    if (item.get('DOCNO') === '*') return;
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            if (item.get('DOCNO') === '*') return;
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否複製?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0010/Copy',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('複製成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnCopy').setDisabled(true);
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
            },
            {
                text: '提出申請',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    if (item.get('DOCNO') === '*') return;
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            if (item.get('DOCNO') === '*') return;
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否提出申請?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0010/UpdateStatus',
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
                                            Ext.getCmp('btnSubmit').setDisabled(true);
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
            },
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
                dataIndex: 'FLOWID',
                width: 100
            },
            {
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 150
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 130
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "<span style='color:red'>申請庫房</span>",
                dataIndex: 'TOWH_NAME',
                width: 130,
                editor: {
                    xtype: 'combo',
                    store: T3Store,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
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
                text: "<span style='color:red'>核撥庫房</span>",
                dataIndex: 'FRWH_NAME',
                width: 140,
                editor: {
                    xtype: 'combo',
                    store: T4Store,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
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
                Ext.getCmp('btnDel').setDisabled(t1Disabled);
                Ext.getCmp('btnCopy').setDisabled(t1Disabled);
                Ext.getCmp('btnSubmit').setDisabled(t1Disabled);
            },
            deselect: function (model, record, index) {
                var t1Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('DOCNO') == '*');
                Ext.getCmp('btnDel').setDisabled(t1Disabled);
                Ext.getCmp('btnCopy').setDisabled(t1Disabled);
                Ext.getCmp('btnSubmit').setDisabled(t1Disabled);
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.getCmp('btnDel2').setDisabled(true);
                T2Load();
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    beforeedit: function (editor, context, eOpts) {
                        if (context.field === 'FRWH_NAME')
                        {
                            Ext.Ajax.request({
                                url: '/api/AB0010/GetGrade',
                                method: reqVal_p,
                                params: {
                                    WH_NO: context.record.get('TOWH')
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    T4Load(data);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                },
                            });
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                    },
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var docno = r.get('DOCNO');
                        var towh = (context.field === 'TOWH_NAME') ? context.value : r.get('TOWH');
                        var frwh = (context.field === 'FRWH_NAME') ? context.value : r.get('FRWH');
                        switch (context.field)
                        {
                            case 'TOWH_NAME':
                                r.set('TOWH', context.value);
                                var cr = T3Store.findRecord('WH_NO', towh);
                                r.set('TOWH_NAME', cr.get('WH_NAME'));
                                break;
                            case 'FRWH_NAME':
                                r.set('FRWH', context.value);
                                var cr = T4Store.findRecord('WH_NO', frwh);
                                r.set('FRWH_NAME', cr.get('WH_NAME'));
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
                                                item.set('APPTIME', dbnr.APPTIME);
                                                //item.load(dbnr);
                                                item.commit();

                                                var recNew = Ext.create('WEBAPP.model.ME_DOCM');
                                                recNew.data.DOCNO = '*';
                                                recNew.data.APP_NAME = userId + ' ' + userName;
                                                recNew.data.APPDEPT_NAME = userInid + ' ' + userInidName;
                                                T1Store.add(recNew);

                                                var recNew2 = Ext.create('WEBAPP.model.ME_DOCD');
                                                recNew2.data.DOCNO = '*';
                                                recNew2.data.SUGGEST_QTY = null;
                                                T2Store.add(recNew2);

                                                msglabel('資料新增成功');

                                                T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew2), 3);
                                                T2LastRec = recNew2;

                                                Ext.getCmp('btnDel').setDisabled(false);
                                                Ext.getCmp('btnSubmit').setDisabled(false);
                                                Ext.getCmp('btnCopy').setDisabled(false);
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
                        else
                        {
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
                                            if (data.success)
                                            {
                                                item.commit();
                                                msglabel('資料更新成功');
                                            }
                                            else
                                            {
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
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        //let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    //name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })

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
            },
            {
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "<span style='color:red'>院內碼</span>",
                dataIndex: 'MMCODE',
                width: 100,
                editor: {
                    xtype: 'mmcodecombo',
                name: 'MMCODE',
                fieldCls: 'required',
                allowBlank: false,
                labelAlign: 'right',
                msgTarget: 'side',
                width: 300,
                //width: 150,

                //限制一次最多顯示10筆
                limit: 10,

                //指定查詢的Controller路徑
                queryUrl: '/api/AB0010/GetMMCodeCombo',

                //查詢完會回傳的欄位
                extraFields: ['MAT_CLASS', 'BASE_UNIT'],

                //查詢時Controller固定會收到的參數
                getDefaultParams: function () {
                    return { WH_NO: T1LastRec.get('FRWH') };
                },
                listeners: {
                    select: function (c, r, i, e) {
                        //選取下拉項目時，顯示回傳值
                        T2LastRec.set('MMNAME_C', r.get('MMNAME_C'));
                        T2LastRec.set('MMNAME_E', r.get('MMNAME_E'));
                    },
                    specialkey: function (field, e) {
                        if (e.getKey() === e.RIGHT) {
                            var editPlugin = this.up().editingPlugin;
                            editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                        }
                    }
                }
            }
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "<span style='color:red'>申請數量</span>",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                editor: {
                    xtype: 'numberfield',
                    fieldCls: 'required',
                    allowBlank: false,
                    minValue: 0,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 3);
                            }
                        }
                    }
                }
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "核撥數量",
                dataIndex: 'APVQTY',
                width: 70,
            },
            {
                text: "核撥日期",
                dataIndex: 'APVTIME',
                width: 70,
            },
            {
                text: "點收數量",
                dataIndex: 'ACKQTY',
                width: 70,
            },
            {
                text: "點收日期",
                dataIndex: 'ACKTIME',
                width: 70,
            },
            {
                text: "建議申請量",
                dataIndex: 'SUGGEST_QTY',
                width: 100,
                renderer: function (val, meta, record) {
                    if (val != null)
                        //return '<a href="javascript:popWinForm(' + val + ",'" + record.data['TMPKEY'] + "')\" >" + val + '</a>';
                        //return '<a href="javascript:popWinForm(' + val + ')" >' + val + '</a>';

                        return "<a href=\"javascript:popWinForm('" + record.data['WH_NO'] + "','" + record.data['MMCODE'] + "')\" >" + val + '</a>';
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            select: function (model, record, index) {
                var t2Disabled = (model.selected.length == 1 && record.get('DOCNO') == '*');
                Ext.getCmp('btnDel2').setDisabled(t2Disabled);
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
                                            DOCNO2: T1LastRec.get('DOCNO'),
                                            FRWH2: T1LastRec.get('FRWH'),
                                            MMCODE: mmcode,
                                            APPQTY: appqty
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                var dbnr = data.etts[0];
                                                item.set('DOCNO', dbnr.DOCNO);
                                                item.set('SEQ', dbnr.SEQ);
                                                item.set('BASE_UNIT', dbnr.BASE_UNIT);
                                                item.set('APVQTY', dbnr.APVQTY);
                                                item.set('ACKQTY', dbnr.ACKQTY);
                                                item.set('ACKTIME', dbnr.ACKTIME);
                                                item.set('SUGGEST_QTY', dbnr.SUGGEST_QTY);
                                                item.commit();

                                                var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                                                recNew.data.DOCNO = '*';
                                                recNew.data.SUGGEST_QTY = null;
                                                T2Store.add(recNew);

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
                                            DOCNO2: T1LastRec.get('DOCNO'),
                                            SEQ: r.get('SEQ'),
                                            FRWH2: T1LastRec.get('FRWH'),
                                            MMCODE: mmcode,
                                            APPQTY: appqty
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                item.commit();
                                                msglabel('資料更新成功');

                                                editor.startEdit(context.rowIdx + 1, 3);
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
                    },
                    canceledit: function (editor, context, eOpts) {
                        //alert('onCancelEdit');
                    }
                }
            })
        ]
    });

    popWinForm = function (wh_no, mmcode) {
        if (!win) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: {
                    type: 'table',
                    columns: 2
                },
                closable: false,
                //html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                //items: [T3Form],
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '安全日',
                        name: 'SAFE_DAY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '安全量',
                        name: 'SAFE_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '作業日',
                        name: 'OPER_DAY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '作業量',
                        name: 'OPER_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '運補日',
                        name: 'SHIP_DAY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '運補量',
                        name: 'SHIP_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '基準量',
                        name: 'HIGH_QTY',
                        labelAlign: 'right',
                        width: 180
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '現有庫存量',
                        name: 'INV_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最低庫存量',
                        name: 'LOW_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '建議申請量',
                        name: 'SUGGEST_QTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '日平均消耗量',
                        name: 'DAVG_USEQTY',
                        labelAlign: 'right',
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '最小撥補量',
                        name: 'MIN_ORDQTY',
                        labelAlign: 'right',
                    },
                ],
                //buttons: [{ xtype: 'label', id: 'DivResult', text: '上傳結果', width: '50%', color: 'blue' },
                buttons: [
                    {
                        id: 'winclosed',
                        disabled: false,
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }
                ]
            });
            //var win = GetPopWin(viewport, popform, '製造商證照（公司登記/工廠登記/營業執照….）', viewport.width - 20, viewport.height - 20);
            var win = GetPopWin(viewport, popform, '明細', 400, 300);

            var QtyStore = viewModel.getStore('SUGGEST_QTY');
            QtyStore.getProxy().setExtraParam("WH_NO", wh_no);
            QtyStore.getProxy().setExtraParam("MMCODE", mmcode);
            QtyStore.load(function () {
                var f = popform.getForm();
                f.findField("SAFE_DAY").setValue(this.getAt(0).get('SAFE_DAY'));
                f.findField("SAFE_QTY").setValue(this.getAt(0).get('SAFE_QTY'));
                f.findField("OPER_DAY").setValue(this.getAt(0).get('OPER_DAY'));
                f.findField("OPER_QTY").setValue(this.getAt(0).get('OPER_QTY'));
                f.findField("SHIP_DAY").setValue(this.getAt(0).get('SHIP_DAY'));
                f.findField("SHIP_QTY").setValue(this.getAt(0).get('SHIP_QTY'));
                f.findField("HIGH_QTY").setValue(this.getAt(0).get('HIGH_QTY'));
                f.findField("INV_QTY").setValue(this.getAt(0).get('INV_QTY'));
                f.findField("LOW_QTY").setValue(this.getAt(0).get('LOW_QTY'));
                f.findField("SUGGEST_QTY").setValue(this.getAt(0).get('SUGGEST_QTY'));
                f.findField("DAVG_USEQTY").setValue(this.getAt(0).get('DAVG_USEQTY'));
                f.findField("MIN_ORDQTY").setValue(this.getAt(0).get('MIN_ORDQTY'));
            });
        }
        win.show();
    }

    Date.prototype.yyymmdd = function () {
        var mm = this.getMonth() + 1; // getMonth() is zero-based
        var dd = this.getDate();

        return [this.getFullYear() - 1911,
        (mm > 9 ? '' : '0') + mm,
        (dd > 9 ? '' : '0') + dd
        ].join('');
    };

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
                width: '70%',
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '30%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '70%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }
        ]
    });
    T3Load();

    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnCopy').setDisabled(true);

    Ext.getCmp('btnDel2').setDisabled(true);
});