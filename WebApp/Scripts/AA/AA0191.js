Ext.onReady(function () {
    Ext.Loader.setConfig({
        enabled: true,
        paths: {
            'WEBAPP': '/Scripts/app'
        }
    });

    var T1Get = '/api/AA0191/Query';
    var T1GetExcel = '/api/AA0191/Excel';
    var T1Set = '/api/AA0191/Update';
    var T2Get = '/api/UR1002/GetMenu';
    var T1ChkADdup = '/api/UR1002/ChkADdup';
    var InidComboGet = '/api/BC0002/GetInidCombo';
    var T1Name = '使用者';
    var T2Name = '功能';

    var T1Rec = 0;
    var T1LastRec = null;
    var IsPageLoad = true;

    function T1Load() {
        T1Rec = 0;
        T1LastRec = null;
        setFormT1a();
        msglabel('');
        T1Store.loadPage(1);
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Menu').unmask();
        //var f = T1Form.getForm();
        //f.findField("TUSER").setValue(T1Store.data.items[0].data.EMAIL);
        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            //if (f.xtype === "combo" || f.xtype === "datefield") {
            //    f.readOnly = true;
            //}
            //else {
            //    f.setReadOnly(true);
            //}
        });

        //T1Form.down('#resetPWD').setVisible(false);
        //T1Form.down('#cancel').hide();
        //T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    //Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var isNew = false;

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'TUSER',
            'INID',
            'INID_NAME',
            'UNA',
            'IDDESC',
            'EMAIL',
            'TEL',
            'EXT',
            'TITLE',
            'FAX',
            'ADUSER',
            'WHITELIST_IP1',
            'WHITELIST_IP2',
            'WHITELIST_IP3',
            { name: 'FL', type: 'int' },
            'PA'
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        //autoLoad: true,
        remoteSort: true,
        sorters: [{ property: 'TUSER', direction: 'ASC' }, { property: 'UNA', direction: 'DESC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    //p0: T1Query.getForm().findField('P0').getValue(),
                    //p1: T1Query.getForm().findField('P1').getValue(),
                    //p2: T1Query.getForm().findField('P2').getValue(),
                    //p3: T1Query.getForm().findField('P3').getValue(),
                    //p4: T1Query.getForm().findField('INID').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                var f = T1Form.getForm();
                f.findField("TUSER").setValue(records[0].data.TUSER);
                f.findField("UNA").setValue(records[0].data.UNA);
                f.findField("EMAIL").setValue(records[0].data.EMAIL);
                if (T2Store.getCount()) T2Store.getRootNode().removeAll();
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                }
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var FL_Query = Ext.create('Ext.data.Store', {
        fields: ['CODE', 'FL'],
        data: [
            { 'CODE': '', 'FL': '' },
            { 'CODE': '1', 'FL': '啟用' },
            { 'CODE': '0', 'FL': '未啟用' }
        ]
    });

    var cbInid_Q = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        labelWidth: 60,
        //queryParam: {
        //GRP_CODE: 'UR_INID',
        //DATA_NAME: 'INID_FLAG'
        //},
        insertEmptyRow: true,
        limit: 20,
        queryUrl: InidComboGet,
        //forceSelection: true
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        padding: 3,
        autoScroll: true,
        border: false,
        items: [{
            xtype: 'panel',
            border: false,
            defaultType: 'textfield',
            padding: '0 0 4 0',
            layout: 'hbox',
            defaults: {
                labelAlign: "right",
                labelWidth: 40
            },
            items: [{
                fieldLabel: '帳號',
                name: 'P0',
                enforceMaxLength: true,
                maxLength: 20,
                labelWidth: 60,
                //padding: '0 4 0 4'
                listeners: {
                    specialkey: function (field, e) {
                        if (e.getKey() === e.ENTER) {
                            T1Load();
                        }
                    }
                }
            }, {
                fieldLabel: '姓名',
                name: 'P1',
                enforceMaxLength: true,
                maxLength: 30,
                //padding: '0 4 0 4'
                listeners: {
                    specialkey: function (field, e) {
                        if (e.getKey() === e.ENTER) {
                            T1Load();
                        }
                    }
                }
            }, cbInid_Q
            ]
        }, {
            xtype: 'panel',
            border: false,
            defaultType: 'textfield',
            layout: 'hbox',
            defaults: {
                labelAlign: "right",
                labelWidth: 40
            },
            items: [
                {
                    fieldLabel: 'AD帳號',
                    name: 'P2',
                    labelWidth: 60,
                    enforceMaxLength: true,
                    maxLength: 30,
                    //padding: '0 4 0 4'
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.ENTER) {
                                T1Load();
                            }
                        }
                    }
                }, {
                    xtype: 'combo',
                    store: FL_Query,
                    id: 'P3',
                    name: 'P3',
                    displayField: 'FL',
                    valueField: 'CODE',
                    fieldLabel: '啟用',
                    queryMode: 'local',
                    autoSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus();
                        msglabel('');
                    }
                }
            ]
        }]
    });

    var cbInid_F = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        //queryParam: {
        //GRP_CODE: 'UR_INID',
        //DATA_NAME: 'INID_FLAG'
        //},
        //insertEmptyRow: true,
        limit: 20,
        queryUrl: InidComboGet,
        fieldCls: 'required',
        //forceSelection: true,
        readOnly: true,
        hidden: true,
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 2
        },
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: "right"
        },
        store: T1Store,
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '帳號',
                name: 'TUSER',
                //displayField: 'TUSER',
                //allowBlank: false,
                //allowOnlyWhitespace: false,
                enforceMaxLength: true,
                maxLength: 20,
                //fieldCls: 'required',
                readOnly: true,
                //listeners: {
                //    blur: function (a, b, c) {
                //        if (isNew) {
                //            var f = T1Form.getForm();
                //            f.findField('PA').setValue(f.findField('TUSER').getValue());
                //        }
                //    }
                //}
            }, {
                xtype: 'component', flex: 1
            }, cbInid_F, {
                xtype: 'displayfield',
                fieldLabel: '姓名',
                name: 'UNA',
                //displayField: 'UNA',
                //allowBlank: false,
                //allowOnlyWhitespace: false,
                enforceMaxLength: true,
                maxLength: 30,
                //fieldCls: 'required',
                readOnly: true
            }, { xtype: 'component', flex: 1 }, {
                //xtype: 'xcombo',
                xtype: 'combo',
                taskFlow: 'TraUR1002GetDeptCombo',
                matchFieldWidth: false,
                listConfig: { width: 310 },
                fieldLabel: '工作單位',
                name: 'WKORG',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: 'E-Mail',
                name: 'EMAIL',
                width: 300,
                colspan: 2,
                readOnly: false
            }, {
                fieldLabel: '電話號碼',
                name: 'TEL',
                readOnly: true,
                hidden: true,
            }, {
                xtype: 'displayfield',
                fieldLabel: '',
            }, {
                xtype: 'component',
                flex: 1,
            }, {
                xtype: 'component',
                flex: 1,
            }, {
                xtype: 'button', width: 100,
                itemId: 'submit', text: '儲存', hidden: false, handler:
                    function () {
                        if (this.up('form').getForm().isValid()) {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                }
                            });
                        }
                        else {
                            Ext.Msg.alert('提醒', '輸入資料格式有誤');//option
                            msglabel('輸入資料格式有誤');
                        }
                    }
            }, {
                fieldLabel: '分機號碼',
                name: 'EXT',
                readOnly: true,
                hidden: true,
            }, {
                //xtype: 'jobtitleselector',
                xtype: 'textfield',
                fieldLabel: '職稱',
                name: 'TITLE',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: '傳真',
                name: 'FAX',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: '使用者密碼',
                inputType: 'password',
                name: 'PA',
                readOnly: true,
                fieldCls: 'readOnly',
                hidden: true,
            }, {
                fieldLabel: 'AD帳號',
                name: 'ADUSER',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: '啟用',
                name: 'FL',
                xtype: 'checkboxfield',
                allowBlank: false,
                allowOnlyWhitespace: false,
                //fieldCls: 'required',
                readOnly: true,
                inputValue: '1',
                uncheckedValue: '0',
                colspan: 2,
                hidden: true,
            }, {
                fieldLabel: '白名單IP1',
                name: 'WHITELIST_IP1',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: '白名單IP2',
                name: 'WHITELIST_IP2',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: '白名單IP3',
                name: 'WHITELIST_IP3',
                readOnly: true,
                hidden: true,
            }, {
                fieldLabel: '人員描述',
                xtype: 'textareafield',
                name: 'IDDESC',
                enforceMaxLength: true,
                maxLength: 50,
                width: 500,
                height: 40,
                readOnly: true,
                colspan: 2,
                hidden: true,
            }, {
                itemId: 'resetPWD',
                xtype: 'button',
                text: '重設密碼',
                hidden: true,
                handler: function () {
                    //Ext.MessageBox.confirm('提示', '將重置密碼並寄送到使用者E-Mail，確認重設密碼？', function (btn, text) {
                    //    if (btn === 'yes') {
                    //        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    //        myMask.show();
                    //        var f = T1Form.getForm();
                    //        //f.findField('PWD').setValue(f.findField('TUSER').getValue());
                    //        var ajaxRequest = $.ajax({
                    //            type: "POST",
                    //            url: '../../../api/UR1002/Reset',
                    //            dataType: "json",
                    //            data: { TUSER: f.findField('TUSER').getValue() }
                    //        })
                    //            .done(function (data, textStatus) {
                    //                //alert(data);
                    //                if (data.success) {
                    //                    Ext.Msg.alert("提示", "密碼重置完成。");
                    //                }
                    //                else {
                    //                    Ext.Msg.alert("錯誤", data.msg);
                    //                }
                    //                myMask.hide();
                    //            })
                    //            .fail(function (data, textStatus) {
                    //                //alert("錯誤:" + data);
                    //                Ext.Msg.alert("錯誤", data.msg);
                    //                myMask.hide();
                    //            });
                    //    }
                    //});
                }
            }, {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    //Added by 思評 2012/11/07
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    //if (T1Form.getForm().findField('x').getValue() == 'U') {
                    //    chkADdup();
                    //}
                    //else {
                    //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    //        if (btn === 'yes') {
                    //            T1Submit();
                    //        }
                    //    });
                    //}
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');//option
                    msglabel('輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Set,
            success: function (form, action) {
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                //var x = f2.findField("x").getValue();
                var x = "U";
                switch (x) {
                    case "I":
                        msglabel('人員資料新增成功');

                        f2.findField('PA').setValue('');

                        var r = Ext.create('T1Model');
                        r.set(action.result.etts[0]);
                        T1Store.insert(0, r);
                        r.commit();

                        T1Grid.getSelectionModel().select(r);
                        T1Store.totalCount = T1Store.totalCount + 1;
                        T1Tool.onLoad();
                        T1Grid.getView().refresh();
                        break;
                    case "U":
                        msglabel('人員資料修改成功');

                        f2.findField('PA').setValue('');

                        var data = action.result;
                        var index = T1Store.indexOf(r);

                        //傳回來的資料有可能是null
                        var r2 = Ext.create('T1Model');
                        r2.set(action.result.etts[0]);

                        //T1Store.data.items[index].data = r2.data;
                        //T1Store.data.items[index].raw = r2.data;
                        //r.commit();
                        //T1Grid.getSelectionModel().select(r);

                        //var f = T1Form.getForm();
                        //f.findField("TUSER").setValue(data.etts[0].EMAIL);
                        break;
                    case "D":
                        msglabel('人員資料刪除成功');
                        var grid1 = T1Grid.getSelectionModel();
                        var next1 = grid1.selectNext();
                        if (!next1) {
                            next1 = grid1.selectPrevious();
                        }
                        T1Store.remove(r);
                        r.commit();

                        T1Store.totalCount = T1Store.totalCount - 1;
                        T1Tool.onLoad();
                        if (next1) {
                        }
                        else {
                            if (isNaN(T1Store.totalCount) || T1Store.totalCount == 0) {
                                T1Rec = 0;
                                T1LastRec = null;
                                setFormT1a();
                            }
                            else {
                                var cp = T1Store.currentPage;
                                if (cp > 1) {
                                    T1Store.loadPage(cp - 1);
                                }
                            }
                        }
                        T1Grid.getView().refresh();
                        break;
                }
                myMask.hide();
                if (x != 'D') {
                    //T1Cleanup();
                    T1Load();
                }
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                }
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        //width: '100%',
        //frame: false,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '../../../api/UR1002/Create';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/UR1002/Update';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '../../../api/UR1002/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                itemId: 'btnExcel', text: '匯出Excel',
                hidden: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: 'UR1002Excel.xls' }); //檔名
                    p.push({ name: 'TS', value: 'u' }); //SQL篩選條件
                    PostForm(T1GetExcel, p);
                }
            },
            {
                itemId: 'btnEncrypt', text: '加密密碼',
                hidden: true,
                handler: function () {
                    //Ext.MessageBox.confirm('提示', '是否加密所有未加密的密碼？', function (btn, text) {
                    //    if (btn === 'yes') {
                    //        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    //        myMask.show();
                    //        var ajaxRequest = $.ajax({
                    //            type: "POST",
                    //            url: '../../../api/UR1002/EncryptAll',
                    //            dataType: "json"
                    //        })
                    //            .done(function (data, textStatus) {
                    //                //alert(data);
                    //                Ext.Msg.alert("提示", "密碼加密完成，共加密" + data.afrs + "筆。");
                    //                myMask.hide();
                    //            })
                    //            .fail(function (data, textStatus) {
                    //                //alert("錯誤:" + data);
                    //                Ext.Msg.alert("錯誤", data.msg);
                    //                myMask.hide();
                    //            });
                    //    }
                    //});
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Menu').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            f.reset();
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            u = f.findField("TUSER");
            u.setReadOnly(false);
            f.findField('ADUSER').setReadOnly(false);
        }
        else {
            u = f.findField('UNA');
            //T1Form.down('#resetPWD').setVisible(true);
        }
        f.findField('x').setValue(x);
        f.findField('INID').setReadOnly(false);
        f.findField('UNA').setReadOnly(false);
        f.findField('IDDESC').setReadOnly(false);
        //f.findField('WKORG').setReadOnly(false);
        f.findField('EMAIL').setReadOnly(false);
        f.findField('TEL').setReadOnly(false);
        f.findField('EXT').setReadOnly(false);
        f.findField('TITLE').setReadOnly(false);
        f.findField('FAX').setReadOnly(false);
        f.findField('FL').setReadOnly(false);
        f.findField('WHITELIST_IP1').setReadOnly(false);
        f.findField('WHITELIST_IP2').setReadOnly(false);
        f.findField('WHITELIST_IP3').setReadOnly(false);
        //f.findField('PA').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        uses: [
            'Ext.ux.exporter.Exporter'
        ],
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],

        columns: [{
            xtype: 'rownumberer'
        }, {
            text: '啟用',
            width: 44,
            stopSelection: false,
            xtype: 'templatecolumn',
            tpl: ['<tpl for="."><input type="checkbox" disabled="disabled" </tpl>',
                '<tpl if="FL == 1">checked="checked"</tpl>',
                '  />'
            ],
            menuDisabled: true
            //}, {
            //    text: '',
            //    dataIndex: 'FS',
            //    width: 20
        }, {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 100,
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 130
        }, {
            text: "AD帳號",
            dataIndex: 'ADUSER',
            width: 100,
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100,
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 150,
        }, {
            text: "人員描述",
            dataIndex: 'IDDESC',
            flex: 1,
            sortable: false
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //treeMask.show();
                if (records.length) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    setFormT1a();
                    T2Store.load({
                        params: {
                            p0: T1LastRec.get('TUSER'),
                            PG: null
                        },
                        callback: function () {
                        }
                    });
                    viewport.down('#form').expand();
                }
            }
        }
    });
    function setFormT1a() {
        var param0 = '';
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('TUSER');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            param0 = T1LastRec.get('TUSER');
        }
        else {
            T1Form.getForm().reset();
        }
        //T2Store.load({
        //    params: {
        //        p0: param0,
        //        PG: null
        //    },
        //    callback: function () {
        //    }
        //});
    }

    //menu
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'FG', type: 'string' },
            { name: 'PG', type: 'string' },
            { name: 'FS', type: 'string' },
            { name: 'text', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.TreeStore', {
        model: 'T2Model',
        defaultRootProperty: 'etts',
        nodeParam: 'PG',
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'Post'
            },
            url: T2Get
        },
        autoLoad: false,
        listeners: {
            beforeload: function (store, operation, options) {
                if (IsPageLoad) { return false; }
                else {
                    if (T1LastRec) {
                        operation.config.params.p0 = T1LastRec.get('TUSER');
                    }
                }
            },
            load: function (store, records, successful, eOpts) {
                treeMask.hide();
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                } else {
                    //T2Tree.collapseAll();
                    T2Tree.expandAll();
                    //var firstNode = T2Tree.getRootNode();
                    //if (firstNode) {
                    //    firstNode.collapse();
                    //    T2Tree.expandNode(firstNode);
                    //}
                }
            }
        }
    });
    var T2Tree = Ext.create('Ext.tree.Panel', {
        id: "navtree",
        title: '',
        header: false,
        collapsible: true,
        useArrows: true,
        rootVisible: false,
        store: T2Store,
        multiSelect: false,
        singleExpand: false,
        loadMask: true,
        cls: 'T2'
    });
    var treeMask = new Ext.LoadMask(T2Tree, { msg: '處理中...' });

    function chkADdup() {
        Ext.Ajax.request({
            url: T1ChkADdup,
            params: {
                p0: T1Form.getForm().findField('TUSER').getValue(),
                p1: T1Form.getForm().findField('ADUSER').getValue(),
                p2: T1Form.getForm().findField('UNA').getValue(),
                p3: T1Form.getForm().findField('FL').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    if (rtnStr == 'DUP') {
                        Ext.MessageBox.confirm('修改', '此AD帳號已有其他設為啟用的帳號,是否確定修改此帳號?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.confirm('修改', '是否確定修改?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                }
            },
            failure: function (response, options) {

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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            width: '60%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid],
            hidden: true,
        },
        {
            itemId: 't2Menu',
            region: 'east',
            layout: 'fit',
            title: T2Name,
            split: true,
            width: '30%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T2Tree],
            hidden: true,
        },
        {
            itemId: 'form',
            region: 'center',
            collapsible: false,
            floatable: true,
            split: true,
            width: '100%',
            minWidth: 120,
            minHeight: 140,
            title: '修改',
            collapsed: false,
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
    msglabel('');
    IsPageLoad = false;
    T1Load();
});