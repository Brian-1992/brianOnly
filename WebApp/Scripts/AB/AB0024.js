Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1LastRec, T2LastRec;
    var T1Name = "繳回作業";
    var T1LastRec;
    var reportUrl = '/Report/A/AB0022.aspx';

    var viewModel = Ext.create('WEBAPP.store.AB.AB0024VM');
    var T1Store = viewModel.getStore('ME_DOCM');
    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;

        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        //T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("p7", T1Query.getForm().findField('P7').getValue());
        T1Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').getValue());
        T1Store.getProxy().setExtraParam("FLOWID", T1Query.getForm().findField('FLOWID').getValue());
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                }
            });
            //T2Store.moveFirst();
        }
    }

    var st_inidcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0024/GetInidComboQ',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0024/GetFrwhComboQ',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_towhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0024/GetTowhComboQ',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
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
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '單據號碼',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'P7',
                id: 'P7',
                padding: '0 4 0 4',
                value: new Date()
            },
            {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'P8',
                id: 'P8',
                padding: '0 4 0 4',
                value: new Date()
            },
            {
                xtype: 'textfield',
                fieldLabel: '申請人員',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                maxLength: 8,
                width: 300,
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'combo',
                fieldLabel: '申請部門',
                name: 'P3',
                id: 'P3',
                store: st_inidcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'combo',
                fieldLabel: '申請庫房',
                name: 'P6',
                id: 'P6',
                store: st_frwhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'combo',
                fieldLabel: '繳回庫房',
                name: 'P5',
                id: 'P5',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                padding: '0 4 0 4'
            },
            {
                xtype: 'checkboxgroup',
                fieldLabel: '狀態',
                name: 'FLOWID',
                columns: 1,
                vertical: true,
                items: [
                    { boxLabel: '申請中', name: 'rb', inputValue: '0401' },
                    { boxLabel: '點收中', name: 'rb', inputValue: '0402', checked: true },
                    { boxLabel: '已結案', name: 'rb', inputValue: '0499' }
                ]
            }
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
                    //setFormVisible(true, false);

                    T1Load();
                    Ext.getCmp('eastform').collapse();
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                }
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
                text: '列印',
                id: 'btnPrint',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要列印的申請單');
                        return;
                    }
                    var docnos = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (docnos != '') {
                            docnos += '|';
                        }
                        docnos += selection[i].data.DOCNO
                    }

                    showReport(docnos);
                }
            },
            {
                text: '結案',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否結案單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AB0024/UpdateStatusBySp',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:結案成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnReturn').setDisabled(true);
                                        } else {
                                            Ext.MessageBox.alert('錯誤', ('發生例外錯誤:' + data.msg));
                                        }
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
                id: 'btnReturn',
                name: 'btnReturn',
                text: '退回',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否退回單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AB0024/Return',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:退回成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnReturn').setDisabled(true);
                                        } else {
                                            Ext.MessageBox.alert('錯誤', ('發生例外錯誤:' + data.msg));
                                        }
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
                text: '效期',
                id: 'exp',
                disabled: true,
                //hidden: true,
                handler: function () {
                    showExpWindow(T1LastRec.data.DOCNO, 'I', viewport);
                }
            }
        ]
    });
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnReturn').setDisabled(true);

    function showReport(docnos) {

        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?DOCNOS=' + docnos
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            //{
            //    dock: 'top',
            //    xtype: 'toolbar',
            //    items: [T1Query]
            //},
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
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 120
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
                width: 150
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            
            {
                text: "申請庫房",
                dataIndex: 'FRWH_NAME',
                width: 150
            },{
                text: "繳回庫房",
                dataIndex: 'TOWH_NAME',
                width: 150
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {

            itemclick: function (self, record, item, index, e, eOpts) {
                
                if (T1Rec == 0) {
                    T1LastRec = null;
                    T2Store.removeAll();
                    return;
                }

                T1LastRec = record;
                
                if (T1LastRec.data.FLOWID.split(' ')[0] == "0402") {
                    Ext.getCmp('btnSubmit').setDisabled(false);
                    Ext.getCmp('btnReturn').setDisabled(false);
                } else {
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnReturn').setDisabled(true);
                }

                T1LastRec = record;
                if (T1LastRec != null && T1LastRec.data.LIST_ID == 'Y') {    //  && T1LastRec.data.LIST_ID == 'Y'
                    Ext.getCmp('exp').setDisabled(false);
                } else {
                    Ext.getCmp('exp').setDisabled(true);
                }

                T2Load();
                

                //setFormVisible(true, false);
                
                //setFormT1a();
                
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;

                Ext.getCmp('btnPrint').disable();
                //if (records.length == 0) {
                //    Ext.getCmp('exp').setDisabled(true);
                //    Ext.getCmp('btnUpdate').setDisabled(true);
                //    Ext.getCmp('btnDel').setDisabled(true);

                //    Ext.getCmp('btnUpdate2').setDisabled(true);
                //    Ext.getCmp('btnDel2').setDisabled(true);
                //    Ext.getCmp('btnAdd2').setDisabled(true);
                //    Ext.getCmp('btnSubmit').setDisabled(true);
                //    Ext.getCmp('btnCopy').setDisabled(true);
                //}
                
                if (records.length == 0) {
                    Ext.getCmp('btnPrint').disable();
                    Ext.getCmp('exp').disable();
                    Ext.getCmp('btnReturn').disable();
                    Ext.getCmp('btnSubmit').disable();
                }
                if (records.length > 0) {
                    Ext.getCmp('btnPrint').enable();
                }
                if (records.length > 1) {
                    Ext.getCmp('exp').setDisabled(true);
                }
            },
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
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
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單位',
                name: 'INID_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回庫房',
                name: 'TOWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請庫房',
                name: 'FRWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }
        ]
    });

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            //viewport.down('#form').expand();
            Tabs.setActiveTab('Form');
            var currentTab = Tabs.getActiveTab();
            currentTab.setTitle('瀏覽');

            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("TOWH").setValue(T1LastRec.data["TOWH_NAME"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

        }

    }

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        hidden: true,
        cls: 'T2b',
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
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單據號號',
                name: 'DOCNO2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APLYITEM_NOTE',
                labelWidth: 90,
                labelAlign: 'right',
                readOnly: true,
                allowBlank: true,
                maxLength: 50,
            }
        ]
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            //viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
        }

    }

    var T2CellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
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
                
                if (mmcode == '' || apvqty == '') return false;
                var items = [];
                Ext.each(modifiedRec, function (item) {
                    if (item.crudState === 'U') {
                        
                        items.push({
                            DOCNO: r.data.DOCNO,
                            MMCODE: r.data.MMCODE,
                            SEQ: r.data.SEQ,
                            APVQTY: apvqty
                        });

                        Ext.Ajax.request({
                            url: '/api/AB0024/UpdateMeDocd',
                            method: reqVal_p,
                            params: {
                                item: Ext.util.JSON.encode(items)
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {

                                    //T2Load();
                                    item.commit();

                                }
                                else {
                                    item.reject();
                                    Ext.Msg.alert('提醒', data.msg, function () {
                                        editor.startEdit(context.rowIdx, 8);
                                    });
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            },
                            //jsonData: data
                        });
                    }
                })
            },
            canceledit: function (editor, context, eOpts) {
                //alert('onCancelEdit');
            }
        }
    })

    var T2RowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '更新',
        cancelBtnText: '取消',
        errorsText: '錯誤訊息',
        dirtyText: '請按更新以修改資料或取消變更',
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                //if (Ext.getCmp('RLNO').getValue() == 'PGRM') {
                //if (Ext.getCmp('viewMode').disabled) {
                //    return false;  // 取消row editing模式
                //} else {
                //var grid = Ext.ComponentQuery.query('grid#grid_contact');
                //    var grid = Ext.ComponentQuery.query('grid')[2];
                //var col = grid.getView().getHeaderCt().getHeaderAtIndex(4);
                //var cols = grid.getView().getHeaderCt().getGridColumns();
                //Ext.each(cols, function (col) {
                //    if (col.text == "原料記號") {
                //        col.setEditor(null);
                //    }
                //});

                //if (context.column.dataIndex == 'PARAU')
                //col.setEditor(null);
                //}
            },
            edit: function (editor, context, eOpts) {
                //var grid = Ext.ComponentQuery.query('#grid_contact')[0];
                //var store = grid.getView().getStore();

                var data = { item: [] };
                data.item.push(context.record.data);

                //alert(context.record.data['NAME_LAST']);
                Ext.Ajax.request({
                    url: '/api/AB0024/UpdateMeDocd',
                    method: reqVal_p,
                    //params: {
                    //    VID5: context.record.data['VID5']
                    //},
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                        }
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    },
                    jsonData: data
                });
            },
            canceledit: function (editor, context, eOpts) {
            }
        }
    });

    Ext.define('MyApp.view.component.Number', {
        override: 'Ext.form.field.Number',
        forcePrecision: false,

        // changing behavior of valueToRaw with forcePrecision
        valueToRaw: function (value) {
            var me = this,
                decimalSeparator = me.decimalSeparator;

            value = me.parseValue(value); // parses value received from the field
            value = me.fixPrecision(value); // applies precision
            value = Ext.isNumber(value) ? value :
                parseFloat(String(value).replace(decimalSeparator, '.'));
            value = isNaN(value) ? '' : String(value).replace('.', decimalSeparator);

            // forcePrecision: true - retains a precision
            // forcePrecision: false - does not retain precision for whole numbers
            if (value == "") {
                return "";

            }
            else {
                return me.forcePrecision ? Ext.Number.toFixed(
                    parseFloat(value),
                    me.decimalPrecision)
                    :
                    parseFloat(
                        Ext.Number.toFixed(
                            parseFloat(value),
                            me.decimalPrecision
                        )
                    );
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T2RowEditing],
        //plugins: [T2CellEditing],
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
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
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
                xtype:"numbercolumn",
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                format: '0.000',
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            //{
            //    xtype: "numbercolumn",
            //    //text: "繳回數量",
            //    text: "<span style='color:red'>繳回數量</span>",
            //    dataIndex: 'APVQTY',
            //    width: 70,
            //    align: 'right',
            //    format: '0.000',
                
            //    editor: {
            //        xtype: 'numberfield',
            //        fieldCls: 'required',
            //        regexText: '數值輸入至小數點後三位',
            //        maxLength: 15,
            //        hideTrigger: true,
            //        maxValue: 99999999999.999,
            //        decimalPrecision: 3,
            //        allowDecimals: true,
            //        keyNavEnabled: false,
            //        allowBlank: false,
            //        forcePrecision: true,
            //        //regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            //        regex: /^[0-9]{1,11}(\.[0-9]{0,3})?$/,
            //        listeners: {
            //            specialkey: function (field, e) {
            //                
            //                if (e.getKey() === e.UP) {
            //                    var editPlugin = this.up().editingPlugin;
            //                    editPlugin.completeEdit();
            //                    var sm = T2Grid.getSelectionModel();
            //                    sm.deselectAll();
            //                    sm.select(editPlugin.context.rowIdx - 1);
            //                    editPlugin.startEdit(editPlugin.context.rowIdx - 1, 8);
            //                }

            //                if (e.getKey() === e.DOWN) {
            //                    var editPlugin = this.up().editingPlugin;
            //                    editPlugin.completeEdit();
            //                    var sm = T2Grid.getSelectionModel();
            //                    
            //                    sm.deselectAll();
            //                    
            //                    sm.select(editPlugin.context.rowIdx + 1);
            //                    editPlugin.startEdit(editPlugin.context.rowIdx + 1, 8);
            //                }
            //            }
            //        }
            //    }
            //},
            {
                //text: "繳回數量",
                text: "<span style='color:red'>繳回數量</span>",
                dataIndex: 'APVQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
                format: '0.000',

                editor: {
                    fieldCls: 'required',
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    allowBlank: false,
                    listeners: {
                        specialkey: function (field, e) {
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
                text: '繳回庫房庫存量',
                dataIndex: 'TOWH_STORE_QTY',
                width: 110,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 70
            },
            {
                header: "",
                flex: 1
            }
        ],
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
                        
                        if (mmcode == '' || apvqty == '') return false;
                        var items = [];
                        Ext.each(modifiedRec, function (item) {
                            if (item.crudState === 'U') {

                                items.push({
                                    DOCNO: r.data.DOCNO,
                                    MMCODE: r.data.MMCODE,
                                    SEQ: r.data.SEQ,
                                    APVQTY: apvqty
                                });

                                Ext.Ajax.request({
                                    url: '/api/AB0024/UpdateMeDocd',
                                    method: reqVal_p,
                                    params: {
                                        item: Ext.util.JSON.encode(items)
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {

                                            //T2Load();
                                            item.commit();

                                        }
                                        else {
                                            item.reject();
                                            Ext.Msg.alert('提醒', data.msg, function () {
                                                editor.startEdit(context.rowIdx, 8);
                                            });
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    },
                                    //jsonData: data
                                });
                            }
                        })
                    },
                    canceledit: function (editor, context, eOpts) {
                        //alert('onCancelEdit');
                    }
                }
            })
        ],
        listeners: {
            beforeedit: function (plugin, context) {
                var tmpArray = T1LastRec.data["FLOWID"].split(' ');
                if (tmpArray[0] !== '0402') {
                    return false;
                }
            },
            itemclick: function (self, record, item, index, e, eOpts) {
                T2LastRec = record;

                if (T2LastRec != null) {
                    //setFormT2a();
                }

                //setFormT2a();

                //setFormVisible(false, true);
            }
        }
    });

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    var Tabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1Query]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
        }]
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
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '查詢',
                //border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                //items: [Tabs]
                items:[T1Query]
            }
        ]
    });
    //Ext.getCmp('eastform').collapse();
    Ext.getCmp('eastform').expand();
    //T1Load();
});