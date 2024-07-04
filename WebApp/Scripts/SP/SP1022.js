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

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
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
        f.findField('F1').setReadOnly(true);
        f.findField('F2').setReadOnly(true);
        f.findField('F3').setReadOnly(true);
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
                text: '查詢', handler: function () {
                    viewport.down('#rightTab').expand();
                    RightTabs.setActiveTab(RightTabs.down('#filter'));
                    if (T1Query.hidden === true) {
                        T1Query.setVisible(true);
                        T2Query.setVisible(false);
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
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#rightTab').expand();
        RightTabs.setActiveTab(RightTabs.down('#form'));
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
                fn: function () { if (T1Form.hidden === true) { T1Form.setVisible(true); T2Form.setVisible(false); } }
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
        T2Grid.down('#add').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
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
        T2Load();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['D1', 'F1', 'D2']
    });

    var T2Query = Ext.widget({
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
            handler: T1Load
        }, {
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P1').focus();
            }
        }]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'D1', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1F1,
                    p1: '%' + T2Query.getForm().findField('P1').getValue() + '%',
                    p2: '%' + T2Query.getForm().findField('P2').getValue() + '%'
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
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
        });
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
        f.findField('D1').setReadOnly(true);
        f.findField('D2').setReadOnly(true);
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
                text: '查詢', handler: function () {
                    viewport.down('#rightTab').expand();
                    RightTabs.setActiveTab(RightTabs.down('#filter'));
                    if (T2Query.hidden === true) {
                        T1Query.setVisible(false);
                        T2Query.setVisible(true);
                    }
                }
            },
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    setFormT2("U", '修編');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
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
        viewport.down('#rightTab').expand();
        RightTabs.setActiveTab(RightTabs.down('#form'));
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T2Model');
            T2Form.loadRecord(r);
            f2.findField('F1').setValue(T1F1);
            u = f2.findField("D1");
            u.setReadOnly(false);
        }
        else {
            u = f2.findField('D2');
        }
        f2.findField('x').setValue(x);
        f2.findField('D2').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        //, {
        //    dock: 'top',
        //    xtype: 'toolbar',
        //    items: [T2Query]
        //}
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
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false); T2Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
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
            var u = f.findField('D1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T2Form.getForm().reset();
        }
    }

    var RightTabs = Ext.createWidget('tabpanel', {
        activeTabe: 0,
        plain: true,
        border: false,
        items: [{
            itemId: 'filter',
            title: '查詢',
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            }
            ,
            items: [T1Query, T2Query],
            listeners: {
                activate: function (tab) {
                    setTimeout(function () {
                        T1Query.getForm().findField('P0').focus();
                    }, 1);
                }
            }
        },
        {
            itemId: 'form',
            title: '修編',
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
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
                    }
                ]
            }]
        },
            {
                itemId: 'rightTab',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [RightTabs]
            }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
