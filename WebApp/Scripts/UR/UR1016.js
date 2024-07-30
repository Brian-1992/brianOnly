Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.FileGridField', 'WEBAPP.form.DocButton']);

Ext.onReady(function () {
    Ext.tip.QuickTipManager.init();

    var T1Create = '/api/UR1016/Create';
    var T1Update = '/api/UR1016/Update';
    var T1Delete = '/api/UR1016/Delete';
    var T1Set = T1Update; //T1Set預設為T1Update
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        autoScroll: true,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 60
        },
        border: false,
        items: [{

            fieldLabel: '文件編號',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 200,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Tool.moveFirst();
                    }
                }
            }

        }, {
            fieldLabel: '文件名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 20,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Tool.moveFirst();
                    }
                }
            }
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
            }
        }
        //    , {
        //    xtype: 'docbutton',
        //    text: '範例文件下載',
        //    documentKey: 'UR1016'
        //}
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.UR1016', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.loadPage(1);
    }

    var T1Form = Ext.widget({
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
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '文件編號',
            name: 'DK',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '文件名稱',
            name: 'DN',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }, {
            xtype: 'textarea',
            fieldLabel: '文件說明',
            name: 'DD',
            enforceMaxLength: true,
            maxLength: 300,
            readOnly: true
        }, {
            xtype: 'filegrid',
            fieldLabel: '文件上傳',
            name: 'UK',
            width: '100%',
            height: 80,
            maxFiles: 1
        }
        ],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T1Submit();
                    }
                }
                );
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Store.removeAll();
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            msglabel('資料新增完成');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('資料修改完成');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('資料刪除完成');
                            break;
                            /*
                            T1Store.remove(r);
                            r.commit();
                            break;
                            */
                    }
                    T1Cleanup();
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
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('DK').setReadOnly(true);
        f.findField('DN').setReadOnly(true);
        f.findField('DD').setReadOnly(true);
        f.findField('UK').setReadOnly(true);
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
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = T1Create;
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = T1Update;
                    setFormT1("U", '修改');
                }
            },
            {
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
        if (x === "I") {
            isNew = true;
            f.reset();
            var r = Ext.create('WEBAPP.model.UR_DOC');
            T1Form.loadRecord(r);
            u = f.findField("DK");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('DN');
        }
        f.findField('x').setValue(x);
        f.findField('DK').setReadOnly(false);
        f.findField('DN').setReadOnly(false);
        f.findField('DD').setReadOnly(false);
        f.findField('UK').setReadOnly(false);
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
            xtype: 'toolbar',
            layout: 'fit',
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
            text: "文件編號",
            dataIndex: 'DK',
            width: 100
        }, {
            text: "文件名稱",
            dataIndex: 'DN',
            width: 100
        }, {
            text: "文件說明",
            dataIndex: 'DD',
            width: 200
        }, {
            xtype: 'datecolumn',
            text: "記錄更新日期/時間",
            dataIndex: 'UPDATE_TIME',
            format: 'X/m/d H:i:s',
            width: 150
        }, {
            text: "記錄更新人員",
            dataIndex: 'UPDATE_USER',
            width: 100
        },
        { sortable: false, flex: 1 }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
                var f = T1Form.getForm();
                f.findField('x').setValue('U');
            /*
                var u = f.findField('DK');
                u.setReadOnly(true);
                u.setFieldStyle('border: 0px');
            */
        }
        else {
            T1Form.getForm().reset();
        }
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
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
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
});
