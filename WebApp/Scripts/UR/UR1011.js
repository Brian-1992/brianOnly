/* File Created: August 21, 2012 */
Ext.onReady(function () {
    //Master
    //var T1Get = '../../../api/flow/process/TraUR10111Get';
    var T1Get = '../../../api/UR1011/QueryM';
    //var T1Set = '../../../api/flow/process/TraUR10111Set';
    var T1Set = '../../../api/UR1011/CreateM';
    var T1Name = "Master";

    //Detail
    //var T11Get = '../../../api/flow/process/TraUR101111Get';
    var T11Get = '../../../api/UR1011/QueryD';
    //var T11Set = '../../../api/flow/process/TraUR101111Set';
    var T11Set = '../../../api/UR1011/CreateD';
    var T11Name = "Detail";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['ID', 'NAME', 'F3']
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [{
            fieldLabel: '參數類別ID',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 50,
            labelWidth: 65,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '參數類別名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50,
            labelWidth: 80,
            padding: '0 4 0 4'
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
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'ID', direction: 'ASC' }],

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
            actionMethods: 'POST',
            url: T1Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Store.loadPage(1);
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
            fieldLabel: '參數類別ID',
            name: 'ID',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '參數類別名稱',
            name: 'NAME',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                //Added by 思評 2012/11/07
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
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('ID').setReadOnly(true);
        f.findField('NAME').setReadOnly(true);
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
                    T1Set = '../../../api/UR1011/CreateM';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    T1Set = '../../../api/UR1011/UpdateM';
                    setFormT1("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '../../../api/UR1011/DeleteM';
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
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            u = f.findField("ID");
            u.setReadOnly(false);
        }
        else {
            u = f.findField('NAME');
        }
        f.findField('x').setValue(x);
        f.findField('NAME').setReadOnly(false);
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
            items: [T1Tool]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Query]
        }
        ],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "參數類別ID",
            dataIndex: 'ID',
            width: 150
        }, {
                text: "參數類別名稱",
            dataIndex: 'NAME',
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }
                }
            },
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
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('ID');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            T1F1 = f.findField('ID').getValue();
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
        }
        T2Load();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['M_ID', 'ID', 'NAME', 'VALUE']
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [{
            fieldLabel: '參數名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50,
            labelWidth: 60,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '參數值',
            name: 'P2',
            enforceMaxLength: true,
            maxLength: 50,
            padding: '0 4 0 4'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: T2Load
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'ID', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue(),
                    p2: T2Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T11Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T2Load() {
        try {
            T2Store.loadPage(1);
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }
    //T1Load();

    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
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
            fieldLabel: '參數類別ID',
            name: 'M_ID',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'readOnly',
            readOnly: true
        }, {
            fieldLabel: '參數ID',
            name: 'ID',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '參數名稱',
            name: 'NAME',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: '參數值',
            name: 'VALUE',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                //Added by 思評 2012/11/07
                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T2Submit();
                    }
                }
                );
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: T2Cleanup
        }]
    });
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = f2.getValues();
                            r.set(v);
                            T2Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            f2.updateRecord(r);
                            r.commit();
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            break;
                    }
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
                    }
                }
            });
        }
    }
    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.findField('ID').setReadOnly(true);
        f.findField('NAME').setReadOnly(true);
        f.findField('VALUE').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    T11Set = '../../../api/UR1011/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    T11Set = '../../../api/UR1011/UpdateD';
                    setFormT2("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T11Set = '../../../api/UR1011/DeleteD';
                            T2Form.getForm().findField('x').setValue('D');
                            T2Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T11Name);
        viewport.down('#form').expand();
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T2Model');
            T2Form.loadRecord(r);
            f2.findField('M_ID').setValue(T1F1);
            u = f2.findField("ID");
            u.setReadOnly(false);
        }
        else {
            u = f2.findField('NAME');
        }
        f2.findField('x').setValue(x);
        f2.findField('NAME').setReadOnly(false);
        f2.findField('VALUE').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        //autoScroll: true,
        cls: 'T2',
        //defaults: {
        //    layout: 'fit'
        //},
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Query]
        }
        ],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "參數ID",
            dataIndex: 'ID',
            width: 100,
            sortable: false
        }, {
            text: "參數名稱",
            dataIndex: 'NAME',
            width: 100,
            sortable: true
        }, {
            text: "參數值",
            dataIndex: 'VALUE',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false); T2Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT2a() {
        T2Grid.down('#edit').setDisabled(T2Rec === 0);
        T2Grid.down('#delete').setDisabled(T2Rec === 0);
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('ID');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T2Form.getForm().reset();
        }
    }

    //view 
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        //enableTabScroll: false,
        layout: 'fit',
        //autoScroll: false,
        //frame:true,
        defaults: {
            //autoScroll: false,
            //autoHeight: true,
            layout: 'fit'
            //closable: false
        },
        items: [{
            title: T11Name,
            items: [T2Grid]
        }]
    });

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
                //  xtype:'container',
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
                    height: '50%',
                    items: [T1Grid]
                },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [TATabs]
                        // items: [T2Grid]
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
            title: '修編',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
