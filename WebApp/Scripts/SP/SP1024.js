/* File Created: August 21, 2012 */
Ext.onReady(function () {
    //Master
    var T1Get = '../../../api/flow/process/TraSP10201Get';
    var T1Set = '../../../api/flow/process/TraSP10201Set';
    var T1Name = "Master";

    //Detail 11
    var T11Get = '../../../api/flow/process/TraSP102011Get';
    var T11Set = '../../../api/flow/process/TraSP102011Set';
    var T11Name = "Detail 1";
    //Detail 12
    var T12Get = '../../../api/flow/process/TraSP102011Get';
    var T12Set = '../../../api/flow/process/TraSP102011Set';
    var T12Name = "Detail 2";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var T12Rec = 0;
    var T12LastRec = null;
    var T1F1 = '';
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
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
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
                text: '查詢', handler: function () {
                    viewport.down('#filter').expand();
                    if (T1Query.hidden === true) {
                        T1Query.setVisible(true);
                        T11Query.setVisible(false);
                        T12Query.setVisible(false);
                    }
                }
            },
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
        var u;
        win1.setTitle(t + T1Name);
        var f = T1Form.getForm();
        if (x === "I") {
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
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T11Form.setVisible(false);
                        T12Form.setVisible(false);
                    }
                }
            },
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
        T11Grid.down('#add').setDisabled(T1Rec === 0);
        T12Grid.down('#add').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            //isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('F1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            T1F1 = f.findField('F1').getValue();
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
        }
        T11Load();
        T12Load();
    }

    //Detail
    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'D1', 'D2']
    });

    var T11Query = Ext.widget({
        xtype: 'form',
        frame: false,
        cls: 'T2b',
        defaultType: 'textfield',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelWidth: 90
        },
        border: false,
        hidden: true,
        items: [{
            fieldLabel: 'Field 1',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50
        }, {
            fieldLabel: 'Field 2',
            name: 'P2',
            enforceMaxLength: true,
            maxLength: 50
        }],

        buttons: [{
            text: '查詢',
            handler: T11Load
        }, {
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P1').focus();
            }
        }]

    });

    var T11Store = Ext.create('Ext.data.Store', {
        model: 'T11Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'D1', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1F1,
                    p1: '%' + T11Query.getForm().findField('P1').getValue() + '%',
                    p2: '%' + T11Query.getForm().findField('P2').getValue() + '%'
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
                root: 'ds.T1',
                totalProperty: 'ds.T1C[0].RC'
            }
        }
    });
    function T11Load() {
        T11Store.load({
            params: {
                start: 0
            }
        });
    }
    //T1Load();

    var T11Form = Ext.widget({
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
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'readOnly',
            readOnly: true
        }, {
            fieldLabel: 'D1 Caption',
            name: 'D1',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: 'D2 Caption',
            name: 'D2',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {

                //Added by 思評 2012/11/07
                //var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                var confirmSubmit = win11.title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T11Submit();
                    }
                }
                );
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T11Cleanup
        }]
    });
    function T11Submit() {
        var f = T11Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T11Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = f2.getValues();
                            r.set(v);
                            T11Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            f2.updateRecord(r);
                            r.commit();
                            break;
                        case "D":
                            T11Store.remove(r);
                            r.commit();
                            break;
                    }
                    T11Cleanup();
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
    function T11Cleanup() {
        hideWin11();
        var f = T11Form.getForm();
        f.reset();
        f.findField('D1').setReadOnly(true);
        f.findField('D2').setReadOnly(true);
        setFormT11a();
    }

    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '查詢', handler: function () {
                    viewport.down('#filter').expand();
                    if (T11Query.hidden === true) {
                        T1Query.setVisible(false);
                        T11Query.setVisible(true);
                        T12Query.setVisible(false);
                    }
                }
            },
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    setFormT11('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    setFormT11("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T11Form.getForm().findField('x').setValue('D');
                            T11Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT11(x, t) {
        var u;
        win11.setTitle(t + T11Name);
        var f2 = T11Form.getForm();
        if (x === "I") {
            var r = Ext.create('T11Model');
            T11Form.loadRecord(r);
            f2.findField('F1').setValue(T1F1);
            u = f2.findField("D1");
            u.setReadOnly(false);
        }
        else {
            u = f2.findField('D2');
        }
        f2.findField('x').setValue(x);
        f2.findField('D2').setReadOnly(false);
        T11Form.down('#cancel').setVisible(true);
        T11Form.down('#submit').setVisible(true);
        win11.defaultFocus = u;
        showWin11();
    }

    var T11Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T11Store,
        plain: true,
        loadMask: true,
        cls: 'T2',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T11Tool]
        }
        ],

        // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: false
        }, {
            text: "D1 Caption",
            dataIndex: 'D1',
            width: 100,
            sortable: true
        }, {
            text: "D2 Caption",
            dataIndex: 'D2',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T11Form.hidden === true) {
                        T1Form.setVisible(false);
                        T11Form.setVisible(true);
                        T12Form.setVisible(false);
                    }
                }
            },

            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
                setFormT11a();
            }
        }
    });
    function setFormT11a() {
        T11Grid.down('#edit').setDisabled(T11Rec === 0);
        T11Grid.down('#delete').setDisabled(T11Rec === 0);
        if (T11LastRec) {
            //isNew = false;
            T11Form.loadRecord(T11LastRec);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('D1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T11Form.getForm().reset();
        }
    }

    //Detail 2
    Ext.define('T12Model', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'D1', 'D2']
    });

    var T12Query = Ext.widget({
        xtype: 'form',
        frame: false,
        cls: 'T2b',
        defaultType: 'textfield',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelWidth: 90
        },
        border: false,
        hidden: true,
        items: [{
            fieldLabel: 'Field 1',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50
        }, {
            fieldLabel: 'Field 2',
            name: 'P2',
            enforceMaxLength: true,
            maxLength: 50
        }],

        buttons: [{
            text: '查詢',
            handler: T12Load
        }, {
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P1').focus();
            }
        }]

    });
    var T12Store = Ext.create('Ext.data.Store', {
        model: 'T12Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'D1', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1F1,
                    p1: '%' + T12Query.getForm().findField('P1').getValue() + '%',
                    p2: '%' + T12Query.getForm().findField('P2').getValue() + '%'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T12Get,
            reader: {
                type: 'json',
                root: 'ds.T1',
                totalProperty: 'ds.T1C[0].RC'
            }
        }
    });
    function T12Load() {
        T12Store.load({
            params: {
                start: 0
            }
        });
    }

    var T12Form = Ext.widget({
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
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'readOnly',
            readOnly: true
        }, {
            fieldLabel: 'D1 Caption',
            name: 'D1',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: 'D2 Caption',
            name: 'D2',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {

                //Added by 思評 2012/11/07
                //var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                var confirmSubmit = win12.title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T12Submit();
                    }
                }
                );
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T12Cleanup
        }]
    });
    function T12Submit() {
        var f = T12Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T12Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T12Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = f2.getValues();
                            r.set(v);
                            T12Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            f2.updateRecord(r);
                            r.commit();
                            break;
                        case "D":
                            T12Store.remove(r);
                            r.commit();
                            break;
                    }
                    T12Cleanup();
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
    function T12Cleanup() {
        hideWin12();
        var f = T12Form.getForm();
        f.reset();
        f.findField('D1').setReadOnly(true);
        f.findField('D2').setReadOnly(true);
        setFormT12a();
    }

    var T12Tool = Ext.create('Ext.PagingToolbar', {
        store: T12Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '查詢', handler: function () {
                    viewport.down('#filter').expand();
                    if (T12Query.hidden === true) {
                        T1Query.setVisible(false);
                        T11Query.setVisible(false);
                        T12Query.setVisible(true);
                    }
                }
            },
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    setFormT12('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    setFormT12("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T12Form.getForm().findField('x').setValue('D');
                            T12Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT12(x, t) {
        var u;
        win12.setTitle(t + T12Name);
        var f2 = T12Form.getForm();
        if (x === "I") {
            var r = Ext.create('T12Model');
            T12Form.loadRecord(r);
            f2.findField('F1').setValue(T1F1);
            u = f2.findField("D1");
            u.setReadOnly(false);
        }
        else {
            u = f2.findField('D2');
        }
        f2.findField('x').setValue(x);
        f2.findField('D2').setReadOnly(false);
        T12Form.down('#cancel').setVisible(true);
        T12Form.down('#submit').setVisible(true);
        win12.defaultFocus = u;
        showWin12();
    }

    var T12Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T12Store,
        plain: true,
        loadMask: true,
        cls: 'T2',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T12Tool]
        }
        ],

        // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: false
        }, {
            text: "D1 Caption",
            dataIndex: 'D1',
            width: 100,
            sortable: true
        }, {
            text: "D2 Caption",
            dataIndex: 'D2',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T12Form.hidden === true) {
                        T1Form.setVisible(false);
                        T11Form.setVisible(false);
                        T12Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                T12Rec = records.length;
                T12LastRec = records[0];
                setFormT12a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT12a() {
        T12Grid.down('#edit').setDisabled(T12Rec === 0);
        T12Grid.down('#delete').setDisabled(T12Rec === 0);
        if (T12LastRec) {
            //isNew = false;
            T12Form.loadRecord(T12LastRec);
            var f = T12Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('D1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T12Form.getForm().reset();
        }
    }

    var RightTabs = Ext.createWidget('tabpanel', {
        activeTabe: 0,
        plain: true,
        border: false,
        items: [{
            title: '查詢',
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            }
            ,
            items: [T1Query, T11Query, T12Query],
            listeners: {
                activate: function (tab) {
                    setTimeout(function () {
                        T1Query.getForm().findField('P0').focus();
                    }, 1);
                }
            }
        }
        ]
    });
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        //enableTabScroll: true,
        defaults: {
            layout: 'fit'
            //autoScroll: true,
            //closabel: false
        },
        items: [{
            title: T11Name,
            items: [T11Grid]
        }, {
            title: T12Name,
            items: [T12Grid]
        }]
    });


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: 'body',
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
                    }
                ]
            }]
        },
            {
                itemId: 'filter',
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
                items: [T1Query, T11Query, T12Query]
            }
        ]
    });

    var win1;
    if (!win1) {
        win1 = Ext.widget('window', {
            title: T1Name,
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

    var win11;
    if (!win11) {
        win11 = Ext.widget('window', {
            title: T11Name,
            closeAction: 'hide',
            width: 250,
            height: 180,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: T11Form,
            defaultFocus: T11Form.getForm().findField('D1')
        });
    }
    function showWin11() {
        if (win11.hidden) { win11.show(); }
    }
    function hideWin11() {
        if (!win11.hidden) {
            win11.hide();
        }
    }

    var win12;
    if (!win12) {
        win12 = Ext.widget('window', {
            title: T12Name,
            closeAction: 'hide',
            width: 250,
            height: 180,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: T12Form,
            defaultFocus: T12Form.getForm().findField('D1')
        });
    }
    function showWin12() {
        if (win12.hidden) { win12.show(); }
    }
    function hideWin12() {
        if (!win12.hidden) {
            win12.hide();
        }
    }

    T1Query.getForm().findField('P0').focus();
});
