/* File Created: August 21, 2012 */
Ext.onReady(function () {
    var T1Get = '../../../api/flow/process/TraSP10101Get';
    var T1Set = '../../../api/flow/process/TraSP10101Set';
    var T1Name = "TableName";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3']
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        cls: 'T1b',
        defaultType: 'textfield',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelWidth: 90
        },
        border: false,
        items: [{
            fieldLabel: 'Field 1',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 50
        }, {
            fieldLabel: 'Field 2',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50
        }],

        buttons: [{
            text: '查詢',
            handler: T1Load
        }, {
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: '%' + T1Query.getForm().findField('P0').getValue() + '%',
                    p1: '%' + T1Query.getForm().findField('P1').getValue() + '%'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T1Get,
            reader: {
                type: 'json',
                root: 'ds.T1',
                totalProperty: 'ds.T1C[0].RC'
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    //T1Load();

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
            fieldLabel: 'F1 Caption',
            name: 'F1',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: 'F2 Caption',
            name: 'F2',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: 'F3 Caption',
            name: 'F3',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                //Added by 思評 2012/11/07
                var confirmSubmit = win1.title.substring(0, 2);
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
                            var v = f2.getValues();
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            f2.updateRecord(r);
                            r.commit();
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            break;
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
        hideWin1();
        var f = T1Form.getForm();
        f.reset();
        f.findField('F1').setReadOnly(true);
        f.findField('F2').setReadOnly(true);
        f.findField('F3').setReadOnly(true);
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
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    setFormT1("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
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
        win1.setTitle(t + T1Name);
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            u = f.findField("F1");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('F2');
        }
        f.findField('x').setValue(x);
        f.findField('F2').setReadOnly(false);
        f.findField('F3').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        win1.defaultFocus = u;
        showWin1();
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
            items: [T1Tool]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
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
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('F1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
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
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '查詢',
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
    //
    //var form = {
    //    xtype: 'form',
    //    //                        layout: {
    //    //                            type: 'vbox'
    //    //                    , align: 'stretch'
    //    //                        },
    //    //                        url: 'api/menu',
    //    border: false,
    //    bodyPadding: 10,

    //    //                        //items: [TAForm],
    //    //                        
    //    html: 'test',
    //    buttons: [{
    //        text: '查詢',
    //        handler: function () {
    //            if (this.up('form').getForm().isValid()) {
    //                // In a real application, this would submit the form to the configured url
    //                this.up('form').getForm().submit();
    //                //                            var frm = this.up('form').getForm();
    //                //                            store.load({ params: {
    //                //                                acct: frm.findField('acct').getValue(),
    //                //                                pwd: frm.findField('pwd').getValue() 
    //                //                            }
    //                //                            });
    //                //this.up('form').getForm().reset();
    //                this.up('window').hide();
    //                //Ext.MessageBox.alert('Thank you!', 'Your inquiry has been sent. We will respond as soon as possible.');
    //            }
    //        }
    //    }, {
    //        text: '取消',
    //        handler: function () {
    //            this.up('form').getForm().reset();
    //            this.up('window').hide();
    //        }
    //    }]
    //};


    var win1;
    if (!win1) {
        win1 = Ext.widget('window', {
            title: '',
            closeAction: 'hide',
            width: 250,
            height: 180,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: T1Form,
            defaultFocus: T1Form.getForm().findField('F1')
        });
    }

    function showWin1() {
        if (win1.hidden) { win1.show(); }
    }
    function hideWin1() {
        if (!win1.hidden) {
            win1.hide();
        }
    }

    T1Query.getForm().findField('P0').focus();

});
