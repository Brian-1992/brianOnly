Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1LastRec, T2LastRec;
    var T1Name = "退藥量修正";
    var T1LastRec = null, T2LastRec = null; T3LastRec = null;
    var T1Set = '';
    var T2Set = '';
    var T3Set = '';

    var viewModel = Ext.create('WEBAPP.store.AB.AB0099VM');
    var T1Store = viewModel.getStore('ME_DOCM');


    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;

        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        //T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        //T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("p7", T1Query.getForm().findField('P7').rawValue);
        T1Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').rawValue);
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
            T2Store.getProxy().setExtraParam("p1", T2Query.getForm().findField('STAT').getValue());
            T2Store.load({
                params: {
                    start: 0
                }
            });
            //T2Store.moveFirst();
        }
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        cls: 'T1b',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '退藥單號',
                name: 'P1',
                id: 'P1',
                maxLength: 21
            },
            {
                xtype: 'datefield',
                fieldLabel: '退藥日期',
                name: 'P7',
                id: 'P7',
                maxLength: 8,
                editable: false
            },
            {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'P8',
                id: 'P8',
                maxLength: 8,
                labelSeparator: '',
                editable: false
            }, 
            {
                xtype: 'textfield',
                fieldLabel: '退藥庫房(病房)',
                name: 'P6',
                id: 'P6',
                maxLength: 8
            },
            {
                xtype: 'textfield',
                fieldLabel: '繳回庫房',
                name: 'P5',
                id: 'P5',
                maxLength: 8
            },
            {
                xtype: 'checkboxgroup',
                fieldLabel: '狀態',
                name: 'FLOWID',
                columns: 1,
                vertical: true,
                //allowBlank: false,
                items: [
                    { boxLabel: '申請中', name: 'rb', inputValue: '1201', checked: true },
                    { boxLabel: '點收中', name: 'rb', inputValue: '1202' },
                    { boxLabel: '已結案', name: 'rb', inputValue: '1299' }
                ]
            }
        ],
        buttons: [
            {
                itemId: 't1query',
                text: '查詢',
                handler: function () {
                    setFormVisible(true, false);

                    T1Load();
                    Ext.getCmp('eastform').collapse();
                }
            },
            {
                itemId: 't1clean',
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
                itemId: 't1edit', text: '修改', disabled: true, xtype: 'hidden', handler: function () {
                    T1Set = '/api/AB0099/UpdateM';
                    setFormT1("U", '修改');
                }
            },
            {
                text: '提出申請',
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

                        Ext.MessageBox.confirm('訊息', '確認是否提出申請,單據號碼<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AB0099/UpdateStatusBySp',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:提出申請成功');
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
                xtype: 'hidden',
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
                                    url: '/api/AB0099/Return',
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
        ]
    });
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnReturn').setDisabled(true);

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        TATabs.down('#Form').setTitle(t);
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM');
            T1Form.loadRecord(r);
            f.clearInvalid();
            //f.findField('DOCNO').setValue('DOCNO');
            f.findField('APP_USER').setValue(session['UserName']);
            getINID(session['UserId']);
            u = f.findField('USEWHERE');
        }
        else {
            u = f.findField('USEWHERE');
        }
        f.findField('x').setValue(x);
        f.findField('DOCNO').setReadOnly(false);
        f.findField('FLOWID').setReadOnly(false);
        f.findField('APPTIME').setReadOnly(false);
        f.findField('FRWH').setReadOnly(false);
        f.findField('TOWH').setReadOnly(false);
        f.findField('APPLY_NOTE').setReadOnly(false);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
                text: "退藥單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                renderer: function (value) {
                    switch (value) {
                        case '1201': return '1201 申請中'; break;
                        case '1202': return '1202 點收中'; break;
                        case '1299': return '1299 已結案'; break;
                        default: return value; break;
                    }
                },  
                width: 120
            },
            //{
            //    text: "申請人員",
            //    dataIndex: 'APP_NAME',
            //    width: 150
            //},
            //{
            //    text: "申請部門",
            //    dataIndex: 'APPDEPT_NAME',
            //    width: 150
            //},
            {
                text: "退藥日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "退藥庫房",
                dataIndex: 'FRWH_NAME',
                width: 150
            },
            {
                text: "繳回庫房",
                dataIndex: 'TOWH_NAME',
                width: 150
            },
            {
                text: "備註",
                dataIndex: 'APPLY_NOTE',
                width: 150
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {

            itemclick: function (self, record, item, index, e, eOpts) {

                T1LastRec = record;
                T1Rec = record.length;
                
                T2Grid.down('#t2query').setDisabled(T1Rec === 0 && !T2Query.getForm().isValid());

                if (T1LastRec.data.FLOWID.split(' ')[0] == "1201") {                    
                    Ext.getCmp('btnSubmit').setDisabled(false);
                    Ext.getCmp('btnReturn').setDisabled(false);
                    T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
                } else {
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnReturn').setDisabled(true);
                    T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
                    Ext.getCmp('btnAccept').setDisabled(true);
                    Ext.getCmp('btnReturnD').setDisabled(true);
                    T2Grid.down('#t2edit').setDisabled(true);
                }

                setFormVisible(true, false);

                setFormT1a();
                T2Load();
            }
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
                fieldLabel: '退藥單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '狀態',
                name: 'FLOWID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true,
                renderer: function (value) {
                    switch (value) {
                        case '1201': return '1201 申請中'; break;
                        case '1202': return '1202 點收中'; break;
                        case '1299': return '1299 已結案'; break;
                        default: return value; break;
                    }
                },  
            },
            {
                xtype: 'displayfield',
                fieldLabel: '退藥日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '退藥庫房',
                name: 'FRWH',
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
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APPLY_NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }
        ],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            TATabs.setActiveTab('Form');
            var currentTab = TATabs.getActiveTab();
            currentTab.setTitle('瀏覽');

            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("TOWH").setValue(T1LastRec.data["TOWH_NAME"]);
            f.findField("FRWH").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("APPLY_NOTE").setValue(T1LastRec.data["APPLY_NOTE"]);

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
                fieldLabel: '退藥單號',
                name: 'DOCNO',
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
                xtype: 'numberfield',
                fieldLabel: '退藥數量',
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true,
                allowDecimals: true,
                decimalPrecision: 0,
                forcePrecision: true
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
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APLYITEM_NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '狀態',
                name: 'STAT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true,
                renderer: function (value) {
                    switch (value) {
                        case 'A': return 'A 已接受'; break;
                        case 'B': return 'B 已退回'; break;
                        case 'C': return 'C 處理中'; break;
                        default: return value; break;
                    }
                }
            }
        ],
        buttons: [{
            itemId: 't2submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    //Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    //    if (btn === 'yes') {
                    //        T2Submit();
                    //    }
                    //});
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定修改?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    });
                    //T2Submit();
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't2cancel', text: '取消', hidden: true, handler: T2Cleanup
        }]
    });


    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    //var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            // 新增後,將key代入查詢條件,只顯示剛新增的資料
                            //var v = action.result.etts[0];
                            //T2Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            //T2Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            //var v = action.result.etts[0];                            
                            //r.set(v);
                            //r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        //case "D":
                        //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        //    r.commit();
                        //    break;
                    }
                    T2Load();
                    T2Cleanup();
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
    }

    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T2Form.down('#t2cancel').hide();
        T2Form.down('#t2submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT2a();
    }

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);
            f.findField('STAT').setValue(T2LastRec.data["STAT"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
        }
    }



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
                    url: '/api/AB0099/UpdateMeDocd',
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

    // 查詢欄位
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        //cls: 'T2b',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            //labelWidth: mLabelWidth,
            //width: mWidth
            labelWidth: 40,
            width: 80
        },
        items: [
            {
                labelAlign: 'right',
                xtype: 'checkboxgroup',
                fieldLabel: '狀態',
                name: 'STAT',
                //id: 'checkboxgroup_d',
                columns: 5,
                vertical: true,
                //allowBlank: false,
                items: [
                    {
                        boxLabel: '已接受', name: 'rb', inputValue: 'A', 
                        handler: function () {
                            setFormVisible(true, false);

                            T2Load();
                            Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        boxLabel: '已退回', name: 'rb', inputValue: 'B',checked: true,
                        handler: function () {
                            setFormVisible(true, false);

                            T2Load();
                            Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        boxLabel: '處理中', name: 'rb', inputValue: 'C',
                        handler: function () {
                            setFormVisible(true, false);

                            T2Load();
                            Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        xtype: 'hidden',
                        itemId: 't2query',
                        text: '查詢',
                        handler: function () {
                            setFormVisible(true, false);

                            T2Load();
                            Ext.getCmp('eastform').collapse();
                        }
                    },
                    {
                        xtype: 'hidden',
                        itemId: 't2clean',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            //f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }
                ]
            },
        ]
    });


    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢條件',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //    }
            //},
            {
                itemId: 't2edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '/api/AB0099/UpdateD';
                    setFormT2("U", '修改');
                }
            },
            {
                xtype: 'hidden',
                text: '接受',
                id: 'btnAccept',
                name: 'btnAccept',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let mmcode = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('MMCODE') + '」<br>';
                        //    mmcode += item.get('MMCODE') + ',';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('MMCODE') + '」<br>';
                            mmcode += item.get('MMCODE') + ',';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否接受下列院內碼?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AB0099/AcceptD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        MMCODE: mmcode
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:接受成功');
                                            T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            Ext.getCmp('btnAccept').setDisabled(true);
                                            Ext.getCmp('btnReturnD').setDisabled(true);
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
                        Ext.MessageBox.alert('錯誤', '尚未選取院內碼');
                        return;
                    }
                }
            },
            {
                xtype: 'hidden',
                text: '退回',
                id: 'btnReturnD',
                name: 'btnReturnD',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let mmcode = '';
                        let docno = '';
                        //selection.map(item => {
                        //    name += '「' + item.get('MMCODE') + '」<br>';
                        //    mmcode += item.get('MMCODE') + ',';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('MMCODE') + '」<br>';
                            mmcode += item.get('MMCODE') + ',';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('訊息', '確認是否退回下列院內碼?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                //var data = { item: [] };
                                //data.item.push(T1LastRec.data["DOCNO"]);
                                Ext.Ajax.request({
                                    url: '/api/AB0099/ReturnD',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        MMCODE: mmcode
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:退回成功');
                                            T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            Ext.getCmp('btnAccept').setDisabled(true);
                                            Ext.getCmp('btnReturnD').setDisabled(true);
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
                        Ext.MessageBox.alert('錯誤', '尚未選取院內碼');
                        return;
                    }
                }
            },
        ]
    });
    Ext.getCmp('btnAccept').setDisabled(true);
    Ext.getCmp('btnReturnD').setDisabled(true);


    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        //viewport.down('#form').setTitle(t + T2Name);
        viewport.down('#form').expand();
        TATabs.setActiveTab('Form');
        TATabs.down('#Form').setTitle(t);
        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f.clearInvalid();
            //f.findField('DOCNO').setValue('DOCNO');
            f.findField('APP_USER').setValue(session['UserName']);
            getINID(session['UserId']);
            //u = f.findField('USEWHERE');
        }
        else {
            //f.findField('DOCNO2').setValue(T1LastRec.data["DOCNO"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField('APLYITEM_NOTE').setValue(T2LastRec.data["APLYITEM_NOTE"]);
        }
        f.findField('x').setValue(x);
        f.findField('DOCNO').setReadOnly(false);
        //f.findField('MMCODE').setReadOnly(false);
        //f.findField('MMNAME_C').setReadOnly(false);
        //f.findField('MMNAME_E').setReadOnly(false);
        f.findField('APPQTY').setReadOnly(false);
        //f.findField('BASE_UNIT').setReadOnly(false);
        f.findField('APLYITEM_NOTE').setReadOnly(false);
        //f.findField('STAT').setReadOnly(false);
        T2Form.down('#t2cancel').setVisible(true);
        T2Form.down('#t2submit').setVisible(true);
        //u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T2RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            },
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
                xtype: 'hidden',
                text: "單據號碼",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 65,
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
                xtype: 'numbercolumn',
                text: "退藥數量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                format: '0.000',
                renderer: function (val, meta, record) {
                    var appqty = record.data.APPQTY;
                    return '<a href=javascript:void(0)>' + appqty + '</a>';
                },
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: 150
            },
            {
                text: "狀態",
                dataIndex: 'STAT',
                width: 100,
                renderer: function (value) {
                    switch (value) {
                        case 'A': return 'A 已接受'; break;
                        case 'B': return 'B 已退回'; break;
                        case 'C': return 'C 處理中'; break;
                        default: return value; break;
                    }
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {                
                if (cellIndex != 6) {
                    return;
                }

                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T2LastRec = record;

                T3Store.removeAll();

                window.show();
                
                T3Load();
            },
            beforeedit: function (plugin, context) {
                var tmpArray = T1LastRec.data["FLOWID"].split(' ');
                if (tmpArray[0] !== '1202') {
                    return false;
                }
            },
            itemclick: function (self, record, item, index, e, eOpts) {
                T2LastRec = record;
                T2Rec = record.length;

                if (T2LastRec != null) {
                    if (T1LastRec.data.FLOWID.split(' ')[0] == "1201" && T2LastRec.data.STAT.split(' ')[0] == "B") {
                        Ext.getCmp('btnAccept').setDisabled(false);
                        Ext.getCmp('btnReturnD').setDisabled(false);
                        T2Grid.down('#t2edit').setDisabled(false);
                    } else {
                        Ext.getCmp('btnAccept').setDisabled(true);
                        Ext.getCmp('btnReturnD').setDisabled(true);
                        T2Grid.down('#t2edit').setDisabled(true);
                    }
                    setFormT2a();
                }

                setFormT2a();

                setFormVisible(false, true);
            }
        }
    });
    
    var T3Store = viewModel.getStore('ME_BACK');
    function T3Load() {        
        //if (T2LastRec != null && T2LastRec.data["DOCNO"] !== '' && T2LastRec.data["RSEQ"] !== '') {
        if (T2LastRec != null && T2LastRec.data["DOCNO"] !== '') {
            T3Store.getProxy().setExtraParam("p0", T2LastRec.data["DOCNO"]);
            T3Store.getProxy().setExtraParam("p1", T2LastRec.data["SEQ"]);
            T3Tool.moveFirst();            
        }
    }

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        border: false,
        plain: true,
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }
        ],
        columns: [
            {
                text: "病房",
                dataIndex: 'NRCODE',
                width: 50
            },
            {
                text: "病床號",
                dataIndex: 'BEDNO',
                width: 50
            },
            {
                text: "病歷號",
                dataIndex: 'MEDNO',
                width: 80
            },
            {
                text: "姓名",
                dataIndex: 'CHINNAME',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'ORDERCODE',
                width: 100
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME',
                width: 300
            },
            {
                text: "開立日期",
                dataIndex: 'BEGINDATETIME',
                width: 100
            },
            {
                text: "DC 日期",
                dataIndex: 'ENDDATETIME',
                width: 100
            },
            {
                text: "劑量",
                dataIndex: 'DOSE',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "頻率",
                dataIndex: 'FREQNO',
                width: 50
            },
            {
                text: "建議退量",
                dataIndex: 'NEEDBACKQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "實際退量",
                dataIndex: 'BACKQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "差異量",
                dataIndex: 'DIFF',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "修改原因",
                dataIndex: 'PHRBACKREASON_NAME',
                width: 100
            },
            {
                text: "退藥日期",
                dataIndex: 'CREATEDATETIME',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        //plugins: [
        //    Ext.create('Ext.grid.plugin.CellEditing', {
        //        clicksToEdit: 1
        //    })
        //],
        listeners: {
            selectionchange: function (model, records) {
                msglabel('訊息區:');
                Ext.getCmp('eastform').expand();
            }
        }
    });

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    var TATabs = Ext.widget('tabpanel', {
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

    var window = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T3Grid],
        width: "600px",
        height: "300px",
        resizable: true,
        modal:true,
        title: "病患資料",
        layout: 'fit',
        buttons: [{
            text: '關閉',
            handler: function () {
                this.up('window').hide();
            }
        }]
    });
    window.hide();

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
                        //border: false,
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
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [TATabs]
            }
        ]
    });
    //Ext.getCmp('eastform').collapse();
    Ext.getCmp('eastform').expand();
    //T1Load();   
    T2Grid.down('#t2query').setDisabled(true);
});