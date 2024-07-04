/* File Created: August 22, 2012 */
Ext.onReady(function () {
    var T1Get = '/api/UR1003/ALL';
    var T1Create = '/api/UR1003/Create';
    var T1Copy = '/api/UR1003/Copy';
    var T1Update = '/api/UR1003/Update';
    var T1Delete = '/api/UR1003/Delete';
    var T1Set = '/api/UR1003/Change';
    var T1Name = '';

    var T1Rec = 0;
    var T1LastRec = null;

    function T1Load() {
        msglabel('訊息區：');
        T1Rec = 0;
        T1LastRec = null;
        setFormT1a();

        T1Store.loadPage(1);
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //Roles
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['RLNO1', 'RLNO', 'RLNA', 'RLDESC', 'ROLE_CREATE_DATE', 'ROLE_CREATE_BY', 'ROLE_MODIFY_DATE', 'ROLE_MODIFY_BY']
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        //autoLoad: true,
        sorters: [{ property: 'RLNO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T1Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        defaults: {
            labelAlign: "right",
            labelWidth: 60
        },

        items: [{
            fieldLabel: '群組代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 10,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Load();
                    }
                }
            }
        }, {
            fieldLabel: '群組名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Load();
                    }
                }
            }
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
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        bodyStyle: 'margin:5px;border:none',
        cls: 'T1b',
        autoScroll: true,
        defaultType: 'textfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelAlign: "right",
            labelWidth: 90
        },

        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '群組代碼',
            name: 'RLNO1'
            , xtype: 'hidden'
        }, {
            fieldLabel: '群組代碼',
            name: 'RLNO',
            enforceMaxLength: true,
            maxLength: 10,
            readOnly: true,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '群組名稱',
            name: 'RLNA',
            enforceMaxLength: true,
            maxLength: 60,
            readOnly: true,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required'
        }, {
            xtype: 'textareafield',
            fieldLabel: '群組說明',
            name: 'RLDESC',
            enforceMaxLength: true,
            readOnly: true,
            height: 100
        }, {
            xtype: 'displayfield',
            fieldLabel: '建立日期',
            name: 'ROLE_CREATE_DATE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '建立人員',
            name: 'ROLE_CREATE_BY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '修改日期',
            name: 'ROLE_MODIFY_DATE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '修改人員',
            name: 'ROLE_MODIFY_BY'
        }, {
            fieldLabel: 'FA',
            name: 'FA',
            xtype: 'hidden'
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                } else {
                    Ext.Msg.alert('警告', 'W0021:輸入資料格式有誤');//option
                    msglabel('W0021:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        var x = f.findField("x").getValue();
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Set,
            success: function (form, action) {
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                switch (f2.findField("x").getValue()) {
                    case "I":
                        msglabel('G0001:資料新增成功');
                        var r = Ext.create('T1Model');
                        r.set(action.result.etts[0]);
                        T1Store.insert(0, r);
                        r.commit();
                        T1Grid.getSelectionModel().select(r);
                        //2013-08-19: 新增或刪除要自動更新總筆數
                        T1Store.totalCount = T1Store.totalCount + 1;
                        T1Tool.onLoad();
                        T1Grid.getView().refresh();
                        break;
                    case "C":
                        msglabel('G0001:資料複製成功');
                        var r = Ext.create('T1Model');
                        r.set(action.result.etts[0]);
                        T1Store.insert(0, r);
                        r.commit();
                        T1Grid.getSelectionModel().select(r);
                        //2013-08-19: 新增或刪除要自動更新總筆數
                        T1Store.totalCount = T1Store.totalCount + 1;
                        T1Tool.onLoad();
                        T1Grid.getView().refresh();
                        break;
                    case "U":
                        msglabel('G0002:資料修改成功');
                        var data = action.result;
                        var index = T1Store.indexOf(r);

                        //傳回來的資料有可能是null
                        var r2 = Ext.create('T1Model');
                        r2.set(action.result.etts[0]);

                        T1Store.data.items[index].data = r2.data;
                        T1Store.data.items[index].raw = r2.data;
                        r.commit();
                        T1Grid.getSelectionModel().select(r);
                        break;
                    case "D":
                        msglabel('G0003:資料刪除成功');
                        //2013-08-19: 解決刪除資料後定位問題
                        var grid1 = T1Grid.getSelectionModel();
                        var next1 = grid1.selectNext();
                        if (!next1) {
                            next1 = grid1.selectPrevious();
                        }
                        T1Store.remove(r);
                        r.commit();

                        //2013-08-19: 新增或刪除要自動更新總筆數
                        T1Store.totalCount = T1Store.totalCount - 1;
                        T1Tool.onLoad();
                        if (next1) {
                        }
                        else {
                            //2013-08-16: 解決若資料已刪除且已無資料,修改/刪除按鈕仍然啟用問題
                            if (isNaN(T1Store.totalCount) || T1Store.totalCount == 0) {
                                T1Rec = 0;
                                T1LastRec = null;
                                setFormT1a();
                            }
                            else {
                                //2013-08-20: 若目前頁已刪除沒有資料時, 需判斷回到上一頁
                                var cp = T1Store.currentPage;
                                if (cp > 1) {
                                    //T11Form.getForm().reset();
                                    //grid1.select(grid1.getLastSelected());
                                    T1Store.loadPage(cp - 1);
                                }
                            }
                        }
                        T1Grid.getView().refresh();
                        break;
                }
                myMask.hide();
                if (x != 'D') {
                    T1Cleanup();
                }
            },
            failure: function (form, action) {
                myMask.hide();
                viewport.down('#t1Grid').unmask();
                var values = f.getValues();
                var err = JSON.parse(action.response.responseText);
                if (err.ExceptionMessage) {
                    msglabel(err.ExceptionMessage);
                    if (err.ExceptionMessage.indexOf('ORA-00001') >= 0) {
                        Ext.Msg.alert('錯誤', values.RLNO + " 群組代碼已存在");
                    }
                } else {
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
                        default:
                            Ext.Msg.alert('錯誤', action.result.msg);
                    }
                }
            }
        });
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('RLNO').setReadOnly(true);
        f.findField('RLNA').setReadOnly(true);
        f.findField('RLDESC').setReadOnly(true);
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            text: '新增', handler: function () {
                T1Set = T1Create;
                setFormT1('I', '新增');
            }
        }, {
                itemId: 'copy', text: '複製轉新增', disabled: true, handler: function () {
                T1Set = T1Copy;
                setFormT1("C", '複製轉新增');
            }
        }, {
            itemId: 'edit', text: '修改', disabled: true, handler: function () {
                T1Set = T1Update;
                setFormT1("U", '修改');
            }
        }, {
            itemId: 'delete', text: '刪除', disabled: true,
            handler: function () {
                Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                    if (btn === 'yes') {
                        T1Set = T1Delete;
                        T1Form.getForm().findField('x').setValue('D');
                        T1Submit();
                    }
                }
                );
            }
        }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === 'I') {
            //isNew = true;
            f.reset();
            //var r = Ext.create('T1Model');
            //T1Form.loadRecord(r);
            u = f.findField("RLNO");
            u.setReadOnly(false);
        }
        else if (x === 'C')
        {
            u = f.findField("RLNO");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('RLNA');
        }
        f.findField('x').setValue(x);
        f.findField('RLNA').setReadOnly(false);
        f.findField('RLDESC').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }
        ],

        // grid columns
        columns: [{
            text: "群組代碼",
            dataIndex: 'RLNO',
            width: 100
        }, {
            text: "群組名稱",
            dataIndex: 'RLNA',
            width: 300
        }, {
            text: "群組說明",
            dataIndex: 'RLDESC',
            flex: 1,
            sortable: false
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#copy').setDisabled(T1Rec === 0);
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            //isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('RLNO1').setValue(f.findField('RLNO').getValue());
            f.findField('x').setValue('U');
        }
        else {
            T1Form.getForm().clear();
        }

        /*
        T1Form.getForm().getFields().each(function (field) {
            field.resetOriginalValue();
        });
        */
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
            width: '70%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            split: true,
            width: '30%',
            minWidth: 120,
            minHeight: 140,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            }
            , items: [T1Form]
        }
        ]
    });

    /*
    T1Form.on('dirtychange', function (basic, dirty, eOpts) {
        window.onbeforeunload = dirty ? my_onbeforeunload : null;
    });
    */
});