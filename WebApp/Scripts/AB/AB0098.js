Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.MMCodeCombo']);

Ext.onReady(function () {
    var T1Create = '/api/AB0098/MasterCreate';
    var T1Update = '/api/AB0098/MasterUpdate';
    var T1Delete = '/api/AB0098/MasterDelete';
    var T2Create = '/api/AB0098/DetailCreate';
    var T2Update = '/api/AB0098/DetailUpdate';
    var T2Delete = '/api/AB0098/DetailDelete';
    var T1Set = '';
    var T1LastRec, T2LastRec;
    //var T1Name = '';
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var viewModel = Ext.create('WEBAPP.store.AB.AB0098VM');

    var hosp_code = '';

    function get_hosp_code() {
        //hosp_code
        Ext.Ajax.request({
            url: '/api/AA0176/GetLoginInfo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    hosp_code = data.msg;
                }
            },
            failure: function (response, options) {

            }
        });
    }

    get_hosp_code();

    var T1Store = viewModel.getStore('ME_DOCM');
    T1Store.on('load', function () {
        var recNew = Ext.create('WEBAPP.model.ME_DOCM');
        recNew.data.DOCNO = '*';
        //recNew.data.APP_NAME = userId + ' ' + userName;
        //recNew.data.APPDEPT_NAME = userInid + ' ' + userInidName;
        T1Store.add(recNew);

        setTimeout(() => T1Grid.editingPlugin.startEdit(T1Store.indexOf(recNew), 3), 1000);
    });
    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());
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
                    if (T1LastRec.get('DOCNO') != '*') {
                        var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                        recNew.data.DOCNO = '*';
                        recNew.data.SUGGEST_QTY = null;
                        T2Store.add(recNew);
                        T2LastRec = recNew;

                        if (T2Store.getCount() == 1)
                            T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);
                        else
                            T2Grid.editingPlugin.startEdit(0, 5);
                    }
                }
            });
        }
    }

    var T3Store = viewModel.getStore('FLOWID');
    function T3Load() {
        T3Store.load({
            params: {
                start: 0
            }
        });
    }

    var T4Store = viewModel.getStore('FRWH');
    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
    }

    var T5Store = viewModel.getStore('TRANSKIND');
    function T5Load() {
        T5Store.load({
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
                        //xtype: 'textfield',
                        xtype: 'datefield',
                        fieldLabel: '調帳日期',
                        name: 'D0',
                        id: 'D0',
                        regex: /\d{7,7}/,
                        labelWidth: 90,
                        width: 175,
                        enforceMaxLength: true, // 限制可輸入最大長度
                        maxLength: 7, // 可輸入最大長度為7
                        padding: '0 4 0 4',
                        value: hosp_code !='0'? new Date() :null,
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Load();
                                }
                            }
                        }
                    },
                    {
                        //xtype: 'textfield',
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: '10px',
                        name: 'D1',
                        id: 'D1',
                        labelSeparator: '',
                        regex: /\d{7,7}/,
                        labelWidth: 15,
                        width: 100,
                        enforceMaxLength: true, // 限制可輸入最大長度
                        maxLength: 7, // 可輸入最大長度為7
                        padding: '0 4 0 4',
                        value: hosp_code != '0' ? new Date() : null,
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Load();
                                }
                            }
                        }
                    },
                    {
                        xtype: 'combo',
                        fieldLabel: '調帳庫別',
                        name: 'P1',
                        id: 'P1',
                        store: T4Store,
                        displayField: 'WH_NAME',
                        valueField: 'WH_NO',
                        editable: false,
                        labelWidth: 60,
                        width: 230,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
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
                        xtype: 'combo',
                        fieldLabel: '狀態',
                        name: 'P2',
                        id: 'P2',
                        store: T3Store,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        editable: false,
                        labelWidth: 40,
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
                        //    tmpArray1 = item.get('FLOWID').split(' ');
                        //    if (item.get('DOCNO') === '*' || tmpArray1[0] === '1799') return;
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            tmpArray1 = item.get('FLOWID').split(' ');
                            if (item.get('DOCNO') === '*' || tmpArray1[0] === '1799') return;
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
                text: '結案',
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

                        Ext.MessageBox.confirm('訊息', '確認是否結案?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0098/UpdateStatusBySP',
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
                                            Ext.getCmp('btnImport').setDisabled(true);
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
                text: '調帳明細',
                id: 'btnShowDetail',
                name: 'btnShowDetail',
                handler: function () {
                    popWinForm('AA0084');
                }
            },
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
                text: "異動單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID_N',
                width: 100
            },
            {
                text: "調帳日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "<span style='color:red'>調帳庫別</span>",
                dataIndex: 'FRWH_NAME',
                width: 200,
                editor: {
                    xtype: 'combo',
                    store: T4Store,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    fieldCls: 'required',
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
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
                tmpArray1 = record.data["FLOWID"].split(' ');
                if (tmpArray1[0] == '1799')
                    t1Disabled = true;
                Ext.getCmp('btnDel').setDisabled(t1Disabled);
                Ext.getCmp('btnSubmit').setDisabled(t1Disabled);
            },
            deselect: function (model, record, index) {
                var t1Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('DOCNO') == '*');
                tmpArray1 = record.data["FLOWID"].split(' ');
                if (tmpArray1[0] == '1799')
                    t1Disabled = true;
                Ext.getCmp('btnDel').setDisabled(t1Disabled);
                Ext.getCmp('btnSubmit').setDisabled(t1Disabled);
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.getCmp('btnDel2').setDisabled(true);
                T2Load();
            },
            beforeedit: function (plugin, context) {
                tmpArray1 = context.record.get("FLOWID").split(' ');
                if (tmpArray1[0] == '1799')
                    return false;
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    //beforeedit: function (editor, context, eOpts) {
                    //    if (context.field === 'FRWH_NAME') {
                    //        Ext.Ajax.request({
                    //            url: '/api/AB0010/GetGrade',
                    //            method: reqVal_p,
                    //            params: {
                    //                WH_NO: context.record.get('TOWH')
                    //            },
                    //            success: function (response) {
                    //                var data = Ext.decode(response.responseText);
                    //                T4Load(data);
                    //            },
                    //            failure: function (response) {
                    //                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    //            },
                    //        });
                    //    }
                    //},
                    validateedit: function (editor, context, eOpts) {
                    },
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var docno = r.get('DOCNO');
                        var frwh = (context.field === 'FRWH_NAME') ? context.value : r.get('FRWH');
                        switch (context.field) {
                            case 'FRWH_NAME':
                                r.set('FRWH', context.value);
                                var cr = T4Store.findRecord('WH_NO', frwh);
                                r.set('FRWH_NAME', cr.get('WH_NAME'));
                                break;
                        }
                        if (docno == '*') //新增
                        {
                            if (frwh == '') return;
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'C') {
                                    Ext.Ajax.request({
                                        url: T1Create,
                                        method: reqVal_p,
                                        params: {
                                            FRWH: frwh
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                var dbnr = data.etts[0];
                                                item.set('DOCNO', dbnr.DOCNO);
                                                item.set('FLOWID', dbnr.FLOWID);
                                                item.set('FLOWID_N', dbnr.FLOWID_N);
                                                item.set('APPTIME', dbnr.APPTIME);
                                                //item.load(dbnr);
                                                item.commit();

                                                var recNew = Ext.create('WEBAPP.model.ME_DOCM');
                                                recNew.data.DOCNO = '*';
                                                //recNew.data.APP_NAME = userId + ' ' + userName;
                                                //recNew.data.APPDEPT_NAME = userInid + ' ' + userInidName;
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
                                            FRWH: frwh
                                            //TOWH: towh
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
                    selectOnFocus: true,
                    allowBlank: false,
                    labelAlign: 'right',
                    msgTarget: 'side',
                    width: 300,
                    //width: 150,

                    //限制一次最多顯示10筆
                    limit: 10,

                    //指定查詢的Controller路徑
                    queryUrl: '/api/AB0098/GetMMCodeCombo',

                    //查詢完會回傳的欄位
                    extraFields: ['MAT_CLASS', 'BASE_UNIT'],

                    //查詢時Controller固定會收到的參數
                    getDefaultParams: function () {
                        return { WH_NO: T1LastRec.get('FRWH') };
                    },
                    listeners: {
                        select: function (c, r, i, e) {

                            //Ext.Ajax.request({
                            //    url: '/api/AB0010/GetQtyInfo',
                            //    method: reqVal_p,
                            //    params: {
                            //        DOCNO: T1LastRec.get('DOCNO'),
                            //        MMCODE: r.get('MMCODE')
                            //    },
                            //    success: function (response) {
                            //        var data = Ext.decode(response.responseText);
                            //        var dbnr = data.etts[0];
                            //        T2LastRec.set('APPQTY', dbnr.SUGGEST_QTY);
                            //        T2LastRec.set('INV_QTY', dbnr.INV_QTY);
                            //        //T2LastRec.set('S_INV_QTY', dbnr.S_INV_QTY);
                            //    },
                            //    failure: function (response) {
                            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            //    },
                            //    //jsonData: data
                            //});
                            //選取下拉項目時，顯示回傳值
                            T2LastRec.set('MMNAME_C', r.get('MMNAME_C'));
                            T2LastRec.set('MMNAME_E', r.get('MMNAME_E'));
                            T2LastRec.set('UP', r.get('M_CONTPRICE'));
                            T2LastRec.set('BASE_UNIT', r.get('BASE_UNIT'));
                        },
                        specialkey: function (field, e) {
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 5);
                            }
                        }
                    }
                }
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "<span style='color:red'>調整量</span>",
                dataIndex: 'APVQTY',
                width: 70,
                align: 'right',
                editor: {
                    fieldCls: 'required',
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    allowBlank: false,
                    //regexText: '只能輸入大於0的數字',
                    //regex: /^[1-9]+$/, // 用正規表示式限制可輸入內容
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 3);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }

                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 5);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 5);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>調帳類別</span>",
                dataIndex: 'TRANSKIND_NAME',
                width: 200,
                editor: {
                    xtype: 'combo',
                    store: T5Store,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    editable: false,
                    fieldCls: 'required',
                    allowBlank: false,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 5);
                            }
                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 8);
                            }

                            //if (e.getKey() === e.UP) {
                            //    var editPlugin = this.up().editingPlugin;
                            //    editPlugin.completeEdit();
                            //    var sm = T2Grid.getSelectionModel();
                            //    sm.deselectAll();
                            //    sm.select(editPlugin.context.rowIdx - 1);
                            //    editPlugin.startEdit(editPlugin.context.rowIdx - 1, 6);
                            //}

                            //if (e.getKey() === e.DOWN) {
                            //    var editPlugin = this.up().editingPlugin;
                            //    editPlugin.completeEdit();
                            //    var sm = T2Grid.getSelectionModel();
                            //    sm.deselectAll();
                            //    sm.select(editPlugin.context.rowIdx + 1);
                            //    editPlugin.startEdit(editPlugin.context.rowIdx + 1, 6);
                            //}
                        }
                    }
                }
            },
            {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "<span style='color:red'>備註</span>",
                dataIndex: 'APLYITEM_NOTE',
                width: 150,
                editor: {
                    xtype: 'textfield',
                    store: T5Store,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }

                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 8);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 8);
                            }
                        }
                    }
                }
            },
            {
                text: "單價",
                dataIndex: 'UP',
                align: 'right',
                width: 70,
            },
            {
                text: "金額",
                dataIndex: 'AMT',
                width: 70,
            },
            {
                text: "庫存量",
                dataIndex: 'INV_QTY',
                align: 'right',
                width: 70,
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            select: function (model, record, index) {
                var t2Disabled = (model.selected.length == 1 && record.get('DOCNO') == '*');
                tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                if (tmpArray1[0] == '1799')
                    t2Disabled = true;
                Ext.getCmp('btnDel2').setDisabled(t2Disabled);
            },
            deselect: function (model, record, index) {
                var t2Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('DOCNO') == '*');;
                tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                if (tmpArray1[0] == '1799')
                    t2Disabled = true;
                Ext.getCmp('btnDel2').setDisabled(t2Disabled);
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
            },
            beforeedit: function (plugin, context) {
                tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                if (tmpArray1[0] == '1799') {
                    return false;
                }

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
                        var apvqty = (context.field === 'APVQTY') ? context.value : r.get('APVQTY');
                        var aplyitem_note = (context.field === 'APLYITEM_NOTE') ? context.value : r.get('APLYITEM_NOTE');
                        var transkind = (context.field === 'TRANSKIND_NAME') ? context.value : r.get('TRANSKIND');
                        var up = (context.field === 'UP') ? context.value : r.get('UP');
                        switch (context.field) {
                            case 'TRANSKIND_NAME':
                                r.set('TRANSKIND', context.value);
                                var cr = T5Store.findRecord('VALUE', transkind);
                                r.set('TRANSKIND_NAME', cr.get('COMBITEM'));
                                break;
                        }
                        if (docno == '*') //新增
                        {
                            
                            if (apvqty != "" && Number(apvqty) <= 0) {
                                Ext.Msg.alert('提醒', '數量不可<=0');
                                return;
                            }
                            if (mmcode == '' || apvqty == '' || transkind == '' || mmcode == null || apvqty == null || transkind == null) return false;
                            
                            Ext.each(modifiedRec, function (item) {
                                if (item.crudState === 'C') {
                                    Ext.Ajax.request({
                                        url: T2Create,
                                        method: reqVal_p,
                                        params: {
                                            DOCNO2: T1LastRec.get('DOCNO'),
                                            FRWH2: T1LastRec.get('FRWH'),
                                            MMCODE: mmcode,
                                            APVQTY: apvqty,
                                            APLYITEM_NOTE: aplyitem_note,
                                            TRANSKIND: transkind,
                                            UP: up,
                                            AMT: apvqty * up
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                var dbnr = data.etts[0];
                                                item.set('DOCNO', dbnr.DOCNO);
                                                item.set('SEQ', dbnr.SEQ);
                                                item.set('APVQTY', dbnr.APVQTY);
                                                item.set('TRANSKIND', dbnr.TRANSKIND);
                                                item.set('APLYITEM_NOTE', dbnr.APLYITEM_NOTE);
                                                item.set('UP', dbnr.UP);
                                                item.set('AMT', dbnr.AMT);
                                                item.set('INV_QTY', dbnr.INV_QTY);
                                                item.commit();

                                                var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                                                recNew.data.DOCNO = '*';
                                                //recNew.data.SUGGEST_QTY = null;
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
                                if (Number(apvqty) <= 0) {
                                    Ext.Msg.alert('提醒', '數量不可<=0');
                                    return;
                                }
                                if (item.crudState === 'U') {
                                    
                                    Ext.Ajax.request({
                                        url: T2Update,
                                        method: reqVal_p,
                                        params: {
                                            DOCNO2: T1LastRec.get('DOCNO'),
                                            SEQ: r.get('SEQ'),
                                            FRWH2: T1LastRec.get('FRWH'),
                                            MMCODE: mmcode,
                                            APVQTY: apvqty,
                                            APLYITEM_NOTE: aplyitem_note,
                                            TRANSKIND: transkind,
                                            UP: up,
                                            AMT: apvqty * up
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                
                                                item.APVQTY = apvqty;
                                                //item.set('UP', dbnr.UP);
                                                //item.set('AMT', dbnr.AMT);
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

    //#region 彈跳視窗

    var callableWin = null;
    popWinForm = function (url) {
        var strUrl = url;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                        if (url == 'GA0004') {
                            T31Load();
                        }
                    }
                }]
            });
            var title = '調帳明細表';
            callableWin = GetPopWin(viewport, popform, title, viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }

    //#endregion

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

    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);

    if (hosp_code != '0') {
        T1Load();
    }
});