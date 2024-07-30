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
    var T2GetExcel = '/api/AB0010/Excel';
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '手動申請';
    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var T1New = false;
    var T1NewFrwh = "";
    var T2mmcode = "";
    var st_Appdept = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0010/GetAppdeptCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Frwh = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0010/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Towh = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0010/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0010/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var st_packunit = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T2mmcode
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0010/GetPackUnitCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }
        //,
        //autoLoad: true
    });
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    var viewModel = Ext.create('WEBAPP.store.AB.AB0010VM');

    var T1Store = viewModel.getStore('ME_DOCM');

    function T1Load() {
        //T1Store.getProxy().setExtraParam("field1", T1Query.getForm().findField('field1').getValue());
        //T1Store.getProxy().setExtraParam("field2", T1Query.getForm().findField('field2').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        //T1Store.getProxy().setExtraParam("p2", userId); 20191211
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        //T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("p7", userInid);
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
                    if (T1LastRec.get('DOCNO') != '*') {
                        var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                        recNew.data.DOCNO = '*';
                        recNew.data.SUGGEST_QTY = null;
                        T2Store.add(recNew);
                        T2LastRec = recNew;
                        if (T2Store.getCount() == 1) {
                            Ext.getCmp('btnExport').setDisabled(true);
                            Ext.getCmp('btnExcel').setDisabled(true);
                            T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);
                        }
                        else {
                            T2Grid.editingPlugin.startEdit(0, 6);
                        }
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
            if (T3Store.getCount() == 0) return;

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
                        xtype: 'combo',
                        fieldLabel: '申請庫房',
                        name: 'P4',
                        id: 'P4',
                        store: st_Towh,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        labelWidth: 60,
                        width: 180,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
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
                        fieldLabel: '核撥庫房',
                        name: 'P6',
                        id: 'P6',
                        store: st_Frwh,
                        queryMode: 'local',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        labelWidth: 60,
                        width: 180,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
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
                            Ext.getCmp('btnExport').setDisabled(true);
                            Ext.getCmp('btnExcel').setDisabled(true);
                            Ext.getCmp('btnImport').setDisabled(true);
                            Ext.getCmp('btnImportAll').setDisabled(true);
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
                        $.map(selection, function (item, key) {
                            if (item.get('DOCNO') === '*') return;
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.Ajax.request({
                            url: '/api/AB0010/CheckFlowidMulti',
                            method: reqVal_p,
                            params: {
                                DOCNO: docno
                            },
                            success: function (response) {
                                
                                var data = Ext.decode(response.responseText);
                                if (data.success == false) {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 3);
                                    });
                                    return;
                                }
                                if (data.msg != '') {
                                    Ext.MessageBox.alert('提醒', data.msg + '<br>將自動重新查詢', function () {
                                        //T1Query.getForm().findField('P1').setValue(T1LastRec.data.DOCNO);
                                        T2Store.removeAll();
                                        T1Load();
                                    });
                                    return;
                                }

                                Ext.MessageBox.confirm('刪除', '是否確定刪除單據號碼?<br>' + name, function (btn, text) {
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
                                                    Ext.getCmp('btnImport').setDisabled(true);
                                                    Ext.getCmp('btnImportAll').setDisabled(true);
                                                    Ext.getCmp('btnExport').setDisabled(true);
                                                    Ext.getCmp('btnExcel').setDisabled(true);
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
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
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
                                            Ext.getCmp('btnImport').setDisabled(true);
                                            Ext.getCmp('btnImportAll').setDisabled(true);
                                            Ext.getCmp('btnExport').setDisabled(true);
                                            Ext.getCmp('btnExcel').setDisabled(true);
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
                        $.map(selection, function (item, key) {
                            if (item.get('DOCNO') === '*') return;
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        });

                        Ext.Ajax.request({
                            url: '/api/AB0010/CheckFlowidMulti',
                            method: reqVal_p,
                            params: {
                                DOCNO: docno
                            },
                            success: function (response) {
                                
                                var data = Ext.decode(response.responseText);
                                if (data.success == false) {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 3);
                                    });
                                    return;
                                }
                                if (data.msg != '') {
                                    Ext.MessageBox.alert('提醒', data.msg + '<br>將自動重新查詢', function () {
                                        //T1Query.getForm().findField('P1').setValue(T1LastRec.data.DOCNO);
                                        T2Store.removeAll();
                                        T1Load();
                                    });
                                    return;
                                }

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
                                                    Ext.getCmp('btnImport').setDisabled(true);
                                                    Ext.getCmp('btnImportAll').setDisabled(true);
                                                    Ext.getCmp('btnExport').setDisabled(true);
                                                    Ext.getCmp('btnExcel').setDisabled(true);
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
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                        });



                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            }, {
                xtype: 'button',
                text: '未核撥列印',
                id: 'T1print_non',
                name: 'T1print_non',
                handler: function () {
                    reportUrl = '/Report/A/AA0015_NON.aspx';
                    st_docno.load();
                    //showReport();
                    showWin4();
                }
            },
            {
                text: '載入低於安全量全品項',
                id: 'btnImport',
                name: 'btnImport',
                handler: function () {
                    if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }

                    if (T2Store.getCount() > 1) { // 因為第一項為*
                        Ext.MessageBox.alert('錯誤', '此單號已存在院內碼，不得再匯入低於安全量全品項');
                        return;
                    }
                    popLoadItem(T1LastRec.data["DOCNO"]);
                }
            },
            {
                text: '載入全庫品項',
                id: 'btnImportAll',
                name: 'btnImportAll',
                handler: function () {
                    if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }

                    if (T2Store.getCount() > 1) { // 因為第一項為*
                        Ext.MessageBox.alert('錯誤', '此單號已存在院內碼，不得再匯入低於安全量全品項');
                        return;
                    }
                    loadAllItem(T1LastRec.data["DOCNO"]);
                }
            },
            {
                xtype: 'button',
                text: '至藥品核撥作業',
                handler: function () {
                    parent.link2('../../../Form/Index/AA0015', '藥品核撥作業(AA0015)');
                }
            },
        ],
        listeners: {
            change: function () {
                T4Store.removeAll();
                var recNew = Ext.create('WEBAPP.model.ME_DOCM');
                recNew.data.DOCNO = '*';
                recNew.data.APPID = userName;
                recNew.data.APP_NAME = userId + ' ' + userName;
                recNew.data.APPDEPT_NAME = userInid + ' ' + userInidName;
                //20191030 ADD
                T1New = true;
                // 申請庫房和核撥庫房只有一筆時,預設代第一筆
                if (T3Store.getCount() == 1) {
                    recNew.data.TOWH_NAME = T3Store.getAt(0).get('WH_NO');
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
                        }
                    });
                }

                T1Store.add(recNew);

                setTimeout(() => T1Grid.editingPlugin.startEdit(T1Store.indexOf(recNew), 7), 1000);
                T1New = false;
            }
        }
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
                xtype: 'rownumberer',
                width: 40
            },
            {
                text: "單據號碼",
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
                dataIndex: 'APPID',
                width: 150
            },
            {
                xtype: 'hidden',
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
                width: 200,
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
                width: 200,
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
                        },
                        focus: function (field, event, eOpts) {
                            
                            if (!T1LastRec.data['TOWH_NAME']) {
                                Ext.MessageBox.alert('提示', '請先選擇申請庫房');
                                field.setValue('');
                                return;
                            }
                            if (T1LastRec.data['TOWH_NAME'] && T4Store.data.items.length == 1)
                                field.select(T4Store.data.items[0].data['WH_NO']);
                        }
                    }
                }
            },
            {
                text: "單據退回備註",
                dataIndex: 'RETURN_NOTE',
                width: 200
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
                Ext.getCmp('btnImport').setDisabled(t1Disabled);
                Ext.getCmp('btnImportAll').setDisabled(t1Disabled);
            },
            deselect: function (model, record, index) {
                var t1Disabled = (model.selected.length == 0) ||
                    (model.selected.length == 1 && model.selected.items[0].get('DOCNO') == '*');
                Ext.getCmp('btnDel').setDisabled(t1Disabled);
                Ext.getCmp('btnCopy').setDisabled(t1Disabled);
                Ext.getCmp('btnSubmit').setDisabled(t1Disabled);
                Ext.getCmp('btnImport').setDisabled(t1Disabled);
                Ext.getCmp('btnImportAll').setDisabled(t1Disabled);
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.getCmp('btnDel2').setDisabled(true);
                Ext.getCmp('btnExport').setDisabled(false);
                Ext.getCmp('btnExcel').setDisabled(false);
                T2Load();
                if (T1LastRec != null) {
                    T4Form.getForm().findField("T4P0").setValue(T1LastRec.data["DOCNO"]);
                }
            }
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    beforeedit: function (editor, context, eOpts) {
                        if (context.field === 'FRWH_NAME') {
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
                        switch (context.field) {
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
                                                recNew.data.APPID = userName;
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
                                                Ext.getCmp('btnImport').setDisabled(false);
                                                Ext.getCmp('btnImportAll').setDisabled(false);
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
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/AB0010/CheckFlowid',
                            method: reqVal_p,
                            params: {
                                p0: T1LastRec.get('DOCNO')
                            },
                            success: function (response) {
                                
                                myMask.hide();
                                var data = Ext.decode(response.responseText);
                                if (data.success == false) {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 3);
                                    });
                                    return;
                                }
                                if (data.msg != '') {
                                    Ext.MessageBox.alert('提醒', data.msg + '，將自動重新查詢', function () {
                                        //T1Query.getForm().findField('P1').setValue(T1LastRec.data.DOCNO);
                                        T2Store.removeAll();
                                        T1Load();
                                    });
                                    return;
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

                                                    for (var i = 0; i < selection.length; i++) {
                                                        var r = T2Store.findRecord('SEQ', selection[i].data.SEQ);
                                                        T2Store.remove(r);
                                                    }

                                                    Ext.getCmp('btnDel2').setDisabled(true);
                                                }
                                            },
                                            failure: function (response) {
                                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                            }
                                        });
                                    }
                                });
                            },
                            failure: function (response) {
                                myMask.hide();
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });


                    }
                }
            }, {
                xtype: 'button',
                text: '列印',
                id: 'btnExport',
                hidden: true,
                name: 'btnExport',
                handler: function () {
                    reportUrl = '/Report/A/AB0012.aspx';
                    showReport12();
                }
            }, {
                xtype: 'button',
                text: '匯出',
                id: 'btnExcel',
                name: 'btnExcel',
                handler: function () {
                    var p = new Array();

                    p.push({ name: 'p0', value: T1LastRec.data["DOCNO"] });

                    PostForm(T2GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }

        ]

    });

    function showReport12() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?'
                + 'docno=' + T1LastRec.data["DOCNO"]
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?'
                //+ 'docno=' + T1LastRec.data["DOCNO"] + '&'
                + 'p0=' + T1LastRec.data["DOCNO"] + '&p1=&p2=&p3=&p4=&p5=1'
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        id: 'Me_docdGrid',
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
                xtype: 'rownumberer',
                width: 40
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
                    matchFieldWidth: false,
                    listConfig: { width: 200 },

                    //限制一次最多顯示10筆
                    limit: 10,

                    //指定查詢的Controller路徑
                    queryUrl: '/api/AB0010/GetMMCodeCombo',

                    //查詢完會回傳的欄位
                    extraFields: ['MAT_CLASS', 'BASE_UNIT'],

                    //查詢時Controller固定會收到的參數
                    getDefaultParams: function () {
                        return { WH_NO: T1LastRec.get('TOWH') };
                    },
                    listeners: {
                        select: function (c, r, i, e) {
                            T2mmcode = r.get("MMCODE");
                            //st_packunit.load();
                            Ext.Ajax.request({
                                url: '/api/AB0010/GetQtyInfo',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1LastRec.get('DOCNO'),
                                    MMCODE: r.get('MMCODE')
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    
                                    var dbnr = data.etts[0];
                                    T2LastRec.set('APPQTY', dbnr.SUGGEST_QTY);
                                    T2LastRec.set('SUGGEST_QTY', dbnr.SUGGEST_QTY);
                                    T2LastRec.set('S_INV_QTY', dbnr.S_INV_QTY);
                                    T2LastRec.set('INV_QTY', dbnr.INV_QTY);
                                    T2LastRec.set('SAFE_QTY', dbnr.SAFE_QTY);
                                    T2LastRec.set('OPER_QTY', dbnr.OPER_QTY);
                                    T2LastRec.set('PACK_QTY', dbnr.PACK_QTY);
                                    T2LastRec.set('PACK_UNIT', dbnr.PACK_UNIT);
                                    T2LastRec.set('E_ORDERDCFLAG', dbnr.E_ORDERDCFLAG);
                                    T2LastRec.set('STORE_LOC', dbnr.STORE_LOC);
                                    T2LastRec.set('WH_NO', T1LastRec.data.TOWH);
                                    T2LastRec.set('ISSPLIT', dbnr.ISSPLIT);
                                    T2LastRec.set('M_AGENNO', dbnr.M_AGENNO);
                                    T2LastRec.set('PACK_TIMES', dbnr.PACK_TIMES);
                                    //T2LastRec.set('S_INV_QTY', dbnr.S_INV_QTY);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                },
                                //jsonData: data
                            });

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
                style: 'text-align:left',
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

                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 7);
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
                text: "<span style='color:red'>備註</span>",
                dataIndex: 'APLYITEM_NOTE',
                width: 70,
                align: 'left',
                editor: {
                    fieldCls: 'required',
                    selectOnFocus: true,
                    fieldStyle: 'text-align:left;',
                    allowBlank: false,
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }

                            //if (e.getKey() === e.RIGHT) {
                            //    var editPlugin = this.up().editingPlugin;
                            //    editPlugin.startEdit(editPlugin.context.rowIdx, 15);
                            //}

                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 7);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 7);
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
                text: "建議申請量",
                dataIndex: 'SUGGEST_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 90,
                renderer: function (val, meta, record) {
                    if (val != null)
                        //return '<a href="javascript:popWinForm(' + val + ",'" + record.data['TMPKEY'] + "')\" >" + val + '</a>';
                        //return '<a href="javascript:popWinForm(' + val + ')" >' + val + '</a>';
                        
                    return "<a href=\"javascript:popWinForm('" + record.data['WH_NO'] + "','" + record.data['MMCODE'] + "')\" >" + val + '</a>';
                }
            },
            //{
            //    text: "建議申請量(新）",
            //    dataIndex: 'APLY_QTY_90',
            //    align: 'right',
            //    style: 'text-align:left',
            //    width: 110,
            //},
            {
                text: "上級庫庫存量",
                dataIndex: 'S_INV_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 100,
            },
            {
                text: "庫存量",
                dataIndex: 'INV_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
            },
            {
                text: "安全量",
                dataIndex: 'SAFE_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
            },
            //{
            //    text: "安全量(新）",
            //    dataIndex: 'SAFE_QTY_90',
            //    align: 'right',
            //    style: 'text-align:left',
            //    width: 90,
            //},
            {
                text: "基準量",
                dataIndex: 'OPER_QTY',//呼叫Get_SAFE_OPER_QTY 取得的round(high_qty)填入
                align: 'right',
                style: 'text-align:left',
                width: 70,
            },
            //{
            //    text: "基準量(新）",
            //    dataIndex: 'HIGH_QTY_90',
            //    align: 'right',
            //    style: 'text-align:left',
            //    width: 90,
            //},
            {
                text: "最低庫存量",
                dataIndex: 'LOW_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 90,
            },
            {
                text: "包裝劑量",
                dataIndex: 'PACK_QTY',
                align: 'right',
                style: 'text-align:left',
                width: 70,
            },
            {
                text: "包裝單位",
                dataIndex: 'PACK_UNIT',
                align: 'right',
                width: 70,
            },
            {
                text: "包裝申領量倍數",
                dataIndex: 'PACK_TIMES',
                align: 'right',
                width: 120,
            },
            //{
            //    text: "<span style='color:red'>包裝單位</span>",
            //    dataIndex: 'PACK_UNIT',
            //    width: 70,
            //    editor: {
            //        xtype: 'combo',
            //        name: 'PACK_UNIT',
            //        store: st_packunit,
            //        queryMode: 'local',
            //        displayField: 'COMBITEM',
            //        valueField: 'VALUE',
            //        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
            //        //readOnly: true,
            //        //typeAhead: true,
            //        //forceSelection: true,
            //        triggerAction: 'all',
            //        fieldCls: 'required',
            //        selectOnFocus: true,
            //        fieldStyle: 'text-align:left;',
            //        allowBlank: false,
            //        listeners: {
            //            specialkey: function (field, e) {
            //                if (e.getKey() === e.LEFT) {
            //                    var editPlugin = this.up().editingPlugin;
            //                    editPlugin.startEdit(editPlugin.context.rowIdx, 7);
            //                }

            //                if (e.getKey() === e.UP) {
            //                    var editPlugin = this.up().editingPlugin;
            //                    editPlugin.completeEdit();
            //                    var sm = T2Grid.getSelectionModel();
            //                    sm.deselectAll();
            //                    sm.select(editPlugin.context.rowIdx - 1);
            //                    editPlugin.startEdit(editPlugin.context.rowIdx - 1, 15);
            //                }

            //                if (e.getKey() === e.DOWN) {
            //                    var editPlugin = this.up().editingPlugin;
            //                    editPlugin.completeEdit();
            //                    var sm = T2Grid.getSelectionModel();
            //                    sm.deselectAll();
            //                    sm.select(editPlugin.context.rowIdx + 1);
            //                    editPlugin.startEdit(editPlugin.context.rowIdx + 1, 15);
            //                }
            //            },
            //            select: function (combo, record, eOpts) {
            //                T2LastRec.set('PACK_QTY', record.get("EXTRA1"));
            //            }
            //        }
            //    }
            //},
            {
                text: "藥品狀態",
                dataIndex: 'DRUGMEMO',
                width: 90,
            },
            {
                text: "藥品停用碼",
                dataIndex: 'E_ORDERDCFLAG',
                width: 90,
            },
            {
                text: "藥庫儲位",
                dataIndex: 'STORE_LOC',
                width: 70,
            },
            {
                text: "核撥數量",
                dataIndex: 'APVQTY',
                align: 'right',
                style: 'text-align:left',
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
                align: 'right',
                style: 'text-align:left',
                width: 70,
            },
            {
                text: "點收日期",
                dataIndex: 'ACKTIME',
                width: 70,
            },
            {
                text: "是否拆單",
                dataIndex: 'ISSPLIT',
                width: 70,
            },
            {
                text: "廠商代碼",
                dataIndex: 'M_AGENNO',
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
                if (T2Rec > 0) {
                    if (T2LastRec.data.MMCODE != "") {
                        T2mmcode = T2LastRec.data.MMCODE;
                        //st_packunit.load();
                    }
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
                        var appqty = (context.field === 'APPQTY') ? context.value : r.get('APPQTY');
                        var itemnote = (context.field === 'APLYITEM_NOTE') ? context.value : r.get('APLYITEM_NOTE');
                        //var packunit = (context.field === 'PACK_UNIT') ? context.value : r.get('PACK_UNIT');
                        //var packqty = (context.field === 'PACK_QTY') ? context.value : r.get('PACK_QTY');

                        Ext.Ajax.request({
                            url: '/api/AB0010/CheckFlowid',
                            method: reqVal_p,
                            params: {
                                p0: T1LastRec.get('DOCNO')
                            },
                            success: function (response) {
                                
                                var data = Ext.decode(response.responseText);
                                if (data.success == false) {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 3);
                                    });
                                    return;
                                }
                                if (data.msg != '') {
                                    Ext.MessageBox.alert('提醒', data.msg + '，將自動重新查詢', function () {
                                        //T1Query.getForm().findField('P1').setValue(T1LastRec.data.DOCNO);
                                        T2Store.removeAll();
                                        T1Load();
                                    });
                                    return;
                                }

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
                                                    APPQTY: appqty,
                                                    APLYITEM_NOTE: itemnote
                                                    //,
                                                    //PACK_UNIT: packunit,
                                                    //PACK_QTY: packqty
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
                                                        item.set('S_INV_QTY', dbnr.S_INV_QTY);
                                                        item.set('INV_QTY', dbnr.INV_QTY);
                                                        item.set('SAFE_QTY', dbnr.SAFE_QTY);
                                                        item.set('OPER_QTY', dbnr.OPER_QTY);
                                                        item.set('PACK_QTY', dbnr.PACK_QTY);
                                                        item.set('PACK_UNIT', dbnr.PACK_UNIT);
                                                        item.set('E_ORDERDCFLAG', dbnr.E_ORDERDCFLAG);
                                                        item.set('APLYITEM_NOTE', dbnr.APLYITEM_NOTE);
                                                        item.set('PACK_TIMES', dbnr.PACK_TIMES);
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
                                                    APPQTY: appqty,
                                                    APLYITEM_NOTE: itemnote
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

    function loadAllItem(docno) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AB0010/LoadAllItem',
            method: reqVal_p,
            params: {
                DOCNO: docno
                //WH_NO: T1LastRec.data["TOWH"]
            },
            //async: true,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.removeAll();
                    //T2Load();
                    //T1Grid.getSelectionModel().deselectAll();
                    //T1Load();
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnCopy').setDisabled(true);

                    T2Store.getProxy().setExtraParam("p0", docno);

                    T2Store.load({
                        params: {
                            start: 0
                        },
                        callback: function () {
                            
                            if (T2Store.getCount() > 1) {
                                Ext.getCmp('btnImport').setDisabled(true);
                                Ext.getCmp('btnImportAll').setDisabled(true);
                                Ext.getCmp('btnExport').setDisabled(true);
                                Ext.getCmp('btnExcel').setDisabled(true);
                                msglabel('匯入成功');
                            }
                            else
                                msglabel('查無資料');

                            if (T1LastRec.get('DOCNO') != '*') {
                                var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                                recNew.data.DOCNO = '*';
                                recNew.data.SUGGEST_QTY = null;
                                T2Store.add(recNew);
                                T2LastRec = recNew;

                                //T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);
                                setTimeout(() => T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3), 1000);
                            }
                        }
                    });

                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);

                myMask.hide();
            },
            failure: function (response) {
                myMask.hide();
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }
    popLoadItem = function (docno) {
        if (!win) {
            var popform = Ext.create({
                xtype: 'form',
                layout: 'form',
                border: false,
                autoScroll: true,
                width: '100%',
                collapsible: true,
                hideCollapseTool: true,
                titleCollapse: true,
                fieldDefaults: {
                    xtype: 'textfield',
                    labelAlign: 'right',
                    labelWidth: false,
                    labelStyle: 'width: 25%',
                    width: '30%'
                },

                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        width: '100%',
                        layout: {
                            type: 'box',
                            vertical: true,
                            align: 'stretch'
                        },
                        items: [
                            {
                                xtype: 'panel',
                                border: false,
                                layout: 'hbox',
                                //width: '100%',
                                items: [{
                                    xtype: 'radiofield',
                                    id: 'P1_SEL',
                                    name: 'P1_SEL',
                                    itemId: 'P1_SEL',
                                    width: '1%',
                                    padding: '4% 0 0 4%',
                                    checked: true,
                                    listeners: {
                                        change: function (field, nVal, oVal, eOpts) {
                                            if (nVal) {
                                                enableField('P1');
                                            }
                                        }
                                    }
                                }, {
                                    xtype: 'displayfield',
                                    width: '99%',
                                    padding: '4% 0 0 10%',
                                    value: '非1~3級管制藥、非疫苗'
                                    //width: '100%',
                                }
                                ]
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '申領品項',
                                itemId: 'IS_AUTO',
                                id: 'IS_AUTO',
                                store: [{ name: '無自動申領品項', value: 'N' }, { name: '自動申領品項', value: 'Y' }],
                                displayField: 'name',
                                valueField: 'value',
                                editable: false,
                                value: 'N',
                                labelAlign: 'right',
                                width: 260
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '院內碼選項',
                                itemId: 'MMCODE_PREFIX',
                                id: 'MMCODE_PREFIX',
                                store: ['003', '004', '005', '006', '007', '008', '009'],
                                value: '005',
                                labelAlign: 'right',
                                editable: false,
                                width: 200
                            },
                        ]
                    },
                    {
                        xtype: 'panel',
                        border: false,
                        layout: {
                            type: 'box',
                            vertical: true,
                            align: 'stretch'
                        },
                        width: '100%',
                        items: [
                            {
                                xtype: 'panel',
                                border: false,
                                layout: 'hbox',
                                width: '100%',
                                items: [{
                                    xtype: 'radiofield',
                                    id: 'P2_SEL',
                                    name: 'P2_SEL',
                                    itemId: 'P2_SEL',
                                    width: '1%',
                                    padding: '4% 0 0 4%',
                                    listeners: {
                                        change: function (field, nVal, oVal, eOpts) {
                                            if (nVal) {
                                                enableField('P2');
                                            }
                                        }
                                    }
                                }, {
                                    xtype: 'displayfield',
                                    value: '1~3級管制藥',
                                    width: '99%',
                                    padding: '4% 0 0 10%',
                                },
                                ]
                            },

                        ]
                    },
                    {
                        xtype: 'panel',
                        border: false,
                        layout: {
                            type: 'box',
                            vertical: true,
                            align: 'stretch'
                        },
                        items: [
                            {
                                xtype: 'panel',
                                border: false,
                                layout: 'hbox',
                                width: '100%',
                                items: [{
                                    xtype: 'radiofield',
                                    id: 'P3_SEL',
                                    name: 'P3_SEL',
                                    itemId: 'P3_SEL',
                                    width: '1%',
                                    padding: '4% 0 0 4%',
                                    listeners: {
                                        change: function (field, nVal, oVal, eOpts) {
                                            if (nVal) {
                                                enableField('P3');
                                            }
                                        }
                                    }
                                }, {
                                    xtype: 'displayfield',
                                    value: '疫苗',
                                    width: '99%',
                                    padding: '4% 0 0 10%',
                                },
                                ]
                            },
                        ]
                    },
                ],
                buttons: [
                    {
                        //id: 'winclosed',
                        disabled: false,
                        text: '載入',
                        handler: function () {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            Ext.Ajax.request({
                                url: '/api/AB0010/LoadLowItem',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    IS_AUTO: popform.getForm().findField('P1_SEL').getValue() == true ? popform.down('#IS_AUTO').getValue() : null,
                                    MMCODE_PREFIX: popform.down('#MMCODE_PREFIX').getValue(),
                                    RESTRICTCODE123: popform.getForm().findField('P2_SEL').getValue() == true ? 'Y' : '',
                                    IS_VACCINE: popform.getForm().findField('P3_SEL').getValue() == true ? 'Y' : '',
                                    //WH_NO: T1LastRec.data["TOWH"]
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T2Store.removeAll();
                                        //T2Load();
                                        //T1Grid.getSelectionModel().deselectAll();
                                        //T1Load();
                                        Ext.getCmp('btnDel').setDisabled(true);
                                        Ext.getCmp('btnSubmit').setDisabled(true);
                                        Ext.getCmp('btnCopy').setDisabled(true);

                                        T2Store.getProxy().setExtraParam("p0", docno);

                                        T2Store.load({
                                            params: {
                                                start: 0
                                            },
                                            callback: function () {
                                                
                                                if (T2Store.getCount() > 1) {
                                                    Ext.getCmp('btnImport').setDisabled(true);
                                                    Ext.getCmp('btnImportAll').setDisabled(true);
                                                    Ext.getCmp('btnExport').setDisabled(true);
                                                    Ext.getCmp('btnExcel').setDisabled(true);
                                                    msglabel('匯入成功');
                                                }
                                                else
                                                    msglabel('查無資料');

                                                if (T1LastRec.get('DOCNO') != '*') {
                                                    var recNew = Ext.create('WEBAPP.model.ME_DOCD');
                                                    recNew.data.DOCNO = '*';
                                                    recNew.data.SUGGEST_QTY = null;
                                                    T2Store.add(recNew);
                                                    T2LastRec = recNew;

                                                    //T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3);
                                                    setTimeout(() => T2Grid.editingPlugin.startEdit(T2Store.indexOf(recNew), 3), 1000);
                                                }
                                            }
                                        });

                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);

                                    myMask.hide();
                                },
                                failure: function (response) {
                                    myMask.hide();
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                            this.up('window').destroy();
                        }
                    },
                    {
                        //id: 'winclosed',
                        disabled: false,
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }
                ]
            });
            var win = GetPopWin(viewport, popform, '明細', 400, 300);
        }
        win.show();

        // 依傳入的欄位是P1(條碼)或P2(院內碼)設定相關欄位屬性
        function enableField(fieldType) {
            if (fieldType == 'P1') {
                Ext.getCmp('P1_SEL').setValue(true);
                Ext.getCmp('P2_SEL').setValue(false);
                Ext.getCmp('P3_SEL').setValue(false);
                Ext.getCmp('IS_AUTO').enable();
                Ext.getCmp('MMCODE_PREFIX').enable();
                Ext.getCmp('IS_AUTO').setValue('N');
                Ext.getCmp('MMCODE_PREFIX').setValue('005');

            }
            else if (fieldType == 'P2') {
                Ext.getCmp('P1_SEL').setValue(false);
                Ext.getCmp('P2_SEL').setValue(true);
                Ext.getCmp('P3_SEL').setValue(false);
                Ext.getCmp('IS_AUTO').disable();
                Ext.getCmp('MMCODE_PREFIX').disable();
                Ext.getCmp('IS_AUTO').setValue('');
                Ext.getCmp('MMCODE_PREFIX').setValue('');

            } else if (fieldType == 'P3') {
                Ext.getCmp('P1_SEL').setValue(false);
                Ext.getCmp('P2_SEL').setValue(false);
                Ext.getCmp('P3_SEL').setValue(true);
                Ext.getCmp('IS_AUTO').disable();
                Ext.getCmp('MMCODE_PREFIX').disable();
                Ext.getCmp('IS_AUTO').setValue('');
                Ext.getCmp('MMCODE_PREFIX').setValue('');

            }
        }
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
    T1Load();
    T3Load();

    var T4Form = Ext.widget({
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
                fieldLabel: '單據號碼',
                labelAlign: 'right',
                name: 'T4P0',
                id: 'T4P0',
                store: st_docno,
                queryMode: 'local',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: '10px',
                name: 'T4P1',
                id: 'T4P1',
                labelSeparator: '',
                store: st_docno,
                queryMode: 'local',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                labelAlign: 'right',
                name: 'T4P2',
                id: 'T4P2',
                vtype: 'dateRange',
                dateRange: { end: 'T4P3' },
                padding: '0 4 0 4',
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: '10px',
                name: 'T4P3',
                id: 'T4P3',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'T4P2' },
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '申請庫房',
                labelAlign: 'right',
                name: 'T4P4',
                id: 'T4P4',
                store: st_Towh,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                padding: '0 4 0 4'
            },
            {
                xtype: 'radiogroup',
                fieldLabel: '排序',
                labelAlign: 'right',
                name: 'T4P5',
                items: [
                    { boxLabel: '儲位', name: 'T4P51', inputValue: '1' },
                    { boxLabel: '院內碼', name: 'T4P51', inputValue: '2', checked: true }
                ],
                padding: '0 4 0 4',
            }
        ],
        buttons: [{
            itemId: 'T4print', text: '列印', handler: function () {
                VvRadio = '1';
                VvRadio = T4Form.getForm().findField('T4P5').getChecked()[0].inputValue;
                showReportT4();
            }
        },
        {
            itemId: 'T4cancel', text: '取消', handler: hideWin4
        }
        ]
    });
    var win4;
    var winActWidth4 = 300;
    var winActHeight4 = 300;
    if (!win4) {
        win4 = Ext.widget('window', {
            title: '列印',
            closeAction: 'hide',
            width: winActWidth4,
            height: winActHeight4,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T4Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth4 > 0) ? winActWidth4 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight4 > 0) ? winActHeight4 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth4 = width;
                    winActHeight4 = height;
                }
            }
        });
    }

    function showWin4() {
        if (win4.hidden) {
            win4.show();
        }
    }
    function hideWin4() {
        if (!win4.hidden) {
            win4.hide();
        }
    }
    function showReportT4() {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?'
                //+ 'docno=' + T1LastRec.data["DOCNO"] + '&'
                + 'fr=AB0010&'
                + 'p0=' + T4Form.getForm().findField('T4P0').getValue() + '&'
                + 'p1=' + T4Form.getForm().findField('T4P1').getValue() + '&'
                + 'p2=' + T4Form.getForm().findField('T4P2').rawValue + '&'
                + 'p3=' + T4Form.getForm().findField('T4P3').rawValue + '&'
                + 'p4=' + T4Form.getForm().findField('T4P4').getValue() + '&'
                + 'p5=' + VvRadio
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnImport').setDisabled(true);
    Ext.getCmp('btnImportAll').setDisabled(true);
    Ext.getCmp('btnCopy').setDisabled(true);

    Ext.getCmp('btnDel2').setDisabled(true);
    Ext.getCmp('btnExport').setDisabled(true);
    Ext.getCmp('btnExcel').setDisabled(true);
});