/* File Created: August 21, 2012 */
Ext.onReady(function () {
    //Master
    var T1Get = '../../../api/flow/process/TraSP10201Get';
    var T1Set = '../../../api/flow/process/TraSP10201Set';
    var T1Name = "Master";

    //Detail
    var T11Get = '../../../api/flow/process/TraSP102011Get';
    var T11Set = '../../../api/flow/process/TraSP102011Set';
    var T11Name = "Detail";

    //Detail
    var T111Get = '../../../api/flow/process/TraSP1020111Get';
    var T111Set = '../../../api/flow/process/TraSP1020111Set';
    var T111Name = "Detail Detail";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var T111Rec = 0;
    var T111LastRec = null;
    var T1F1 = '';
    var T11D1 = '';

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
        }
        //, {
        //    xtype: 'button',
        //    text: '查詢',
        //    handler: T1Load
        //}, {
        //    xtype: 'button',
        //    text: '清除',
        //    handler: function () {
        //        var f = this.up('form').getForm();
        //        f.reset();
        //        f.findField('P0').focus();
        //    }
        //}
        ],
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
                        T111Query.setVisible(false);
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
        }
        //, {
        //    dock: 'top',
        //    xtype: 'toolbar',
        //    items: [T1Query]
        //}
        ],

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
                        T111Form.setVisible(false);
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
        T11Grid.down('#add').setDisabled(T1Rec === 0);
        T111Grid.down('#add').setDisabled(T1Rec === 0);
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
    }

    //Detail
    var T11Rec = 0;
    var T11LastRec = null;

    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['D1', 'F1', 'D2']
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
        }
        //, {
        //    xtype: 'button',
        //    text: '查詢',
        //    handler: T11Load
        //}, {
        //    xtype: 'button',
        //    text: '清除',
        //    handler: function () {
        //        var f = this.up('form').getForm();
        //        f.reset();
        //        f.findField('P1').focus();
        //    }
        //}
        ],
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
        try {
            T11Store.load({
                params: {
                    start: 0
                }
            });
        }
        catch (e) {
            alert("T11Load Error:" + e);
        }
    }
    //T11Load();

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
                        T111Query.setVisible(false);
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
        //, {
        //    dock: 'top',
        //    xtype: 'toolbar',
        //    items: [T11Query]
        //}
        ],

        //                // grid columns
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
                        T111Form.setVisible(false);
                    }
                }
            },

            selectionchange: function (model, records) {
                T11Rec = records.length;
                T11LastRec = records[0];
                setFormT11a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT11a() {
        T11Grid.down('#edit').setDisabled(T11Rec === 0);
        T11Grid.down('#delete').setDisabled(T11Rec === 0);
        T11Grid.down('#add').setDisabled(T1Rec === 0);
        T111Grid.down('#add').setDisabled(T1Rec === 0);
        if (T11LastRec) {
            //isNew = false;
            T11Form.loadRecord(T11LastRec);
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('D1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            T1F1 = f.findField('F1').getValue();
            T11D1 = f.findField('D1').getValue();
        }
        else {
            T11Form.getForm().reset();
            T1F1 = '';
            T11D1 = '';
        }
        T111Load();
    }

    //Detail 2
    var T111Rec = 0;
    var T111LastRec = null;

    Ext.define('T111Model', {
        extend: 'Ext.data.Model',
        fields: ['D1', 'F1', 'D11', 'D12']
    });

    var T111Query = Ext.widget({
        xtype: 'form',
        frame: false,
        cls: 'T3b',
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
        }
        //, {
        //    xtype: 'button',
        //    text: '查詢',
        //    handler: T111Load
        //}, {
        //    xtype: 'button',
        //    text: '清除',
        //    handler: function () {
        //        var f = this.up('form').getForm();
        //        f.reset();
        //        f.findField('P0').focus();
        //    }
        //}
        ],
        buttons: [{
            text: '查詢',
            handler: T111Load
        }, {
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P1').focus();
            }
        }]
    });

    var T111Store = Ext.create('Ext.data.Store', {
        model: 'T111Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'D11', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1F1,
                    p1: T11D1,
                    p2: '%' + T11Query.getForm().findField('P1').getValue() + '%',
                    p3: '%' + T11Query.getForm().findField('P2').getValue() + '%'
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T111Get,
            reader: {
                type: 'json',
                root: 'ds.T1',
                totalProperty: 'ds.T1C[0].RC'
            }
        }
    });
    function T111Load() {
        try {
            T111Store.load({
                params: {
                    start: 0
                }
            });
        }
        catch (e) {
            alert("T111Load Error:" + e);
        }
    }
    //T111Load();

    var T111Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T3b',
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
            fieldLabel: 'D11 Caption',
            name: 'D11',
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: 'D12 Caption',
            name: 'D12',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                //Added by 思評 2012/11/07
                var confirmSubmit = win111.title.substring(0, 2);
                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    if (btn === 'yes') {
                        T111Submit();
                    }
                }
                );
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T111Cleanup
        }]
    });
    function T111Submit() {
        var f = T111Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T111Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T111Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = f2.getValues();
                            r.set(v);
                            T111Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            f2.updateRecord(r);
                            r.commit();
                            break;
                        case "D":
                            T111Store.remove(r);
                            r.commit();
                            break;
                    }
                    T111Cleanup();
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
    function T111Cleanup() {
        hideWin111();
        var f = T111Form.getForm();
        f.reset();
        f.findField('D11').setReadOnly(true);
        f.findField('D12').setReadOnly(true);
        setFormT111a();
    }

    var T111Tool = Ext.create('Ext.PagingToolbar', {
        store: T111Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '查詢', handler: function () {
                    viewport.down('#filter').expand();
                    if (T111Query.hidden === true) {
                        T1Query.setVisible(false);
                        T11Query.setVisible(false);
                        T111Query.setVisible(true);
                    }
                }
            },
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    setFormT111('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    setFormT111("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T111Form.getForm().findField('x').setValue('D');
                            T111Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    function setFormT111(x, t) {
        var u;
        win111.setTitle(t + T111Name);
        var f2 = T111Form.getForm();
        if (x === "I") {
            var r = Ext.create('T111Model');
            T111Form.loadRecord(r);
            f2.findField('F1').setValue(T1F1);
            f2.findField('D1').setValue(T11D1);
            u = f2.findField('D12');
            u.setReadOnly(false);
            u = f2.findField("D11");
            u.setReadOnly(false);
        }
        else {
            u = f2.findField('D11');
            u.setReadOnly(true);
            u = f2.findField('D12');
            u.setReadOnly(false);
        }
        f2.findField('x').setValue(x);
        //f2.findField('D2').setReadOnly(false);
        T111Form.down('#cancel').setVisible(true);
        T111Form.down('#submit').setVisible(true);
        win111.defaultFocus = u;
        showWin111();
    }

    var T111Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T111Store,
        plain: true,
        loadMask: true,
        cls: 'T3',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T111Tool]
        }
        //, {
        //    dock: 'top',
        //    xtype: 'toolbar',
        //    items: [T111Query]
        //}
        ],

        //                // grid columns
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
            text: "D11 Caption",
            dataIndex: 'D11',
            width: 100,
            sortable: true
        }, {
            text: "D12 Caption",
            dataIndex: 'D12',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T111Form.hidden === true) {
                        T1Form.setVisible(false);
                        T11Form.setVisible(false);
                        T111Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                T111Rec = records.length;
                T111LastRec = records[0];
                setFormT111a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT111a() {
        T111Grid.down('#edit').setDisabled(T111Rec === 0);
        T111Grid.down('#delete').setDisabled(T111Rec === 0);
        if (T111LastRec) {
            //isNew = false;
            T111Form.loadRecord(T111LastRec);
            var f = T111Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('D11');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T111Form.getForm().reset();
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
            items: [T1Query, T11Query, T111Query],
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
    var TAATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        //enableTabScroll: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
            //autoScroll: true,
            //closabel: false
        },
        items: [{
            title: T111Name,
            items: [T111Grid]
        }]
    });

    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        //enableTabScroll: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
            //autoScroll: true,
            //closabel: false
        },
        items: [{
            title: T11Name,
            items: [T11Grid]
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
                    flex: 1,
                    split: true,
                    items: [T1Grid]
                },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        flex: 1,
                        split: true,
                        items: [TATabs]
                    }, {
                        region: 'south',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        flex: 1,
                        split: true,
                        items: [TAATabs]
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
                items: [T1Query, T11Query, T111Query]
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

    var win111;
    if (!win111) {
        win111 = Ext.widget('window', {
            title: T111Name,
            closeAction: 'hide',
            width: 250,
            height: 180,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: T111Form,
            defaultFocus: T111Form.getForm().findField('D1')
        });
    }
    function showWin111() {
        if (win111.hidden) { win111.show(); }
    }
    function hideWin111() {
        if (!win111.hidden) {
            win111.hide();
        }
    }

    T1Query.getForm().findField('P0').focus();
});
