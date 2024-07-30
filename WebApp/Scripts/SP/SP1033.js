/* File Created: August 21, 2012 */
Ext.onReady(function () {
    var TA = "S133A";
    var TAA = "S133AA";
    var TAB = "S133AB";
    var TAAA = "S133AAA";
    var TABA = "S133ABA";
    var TATitle = "Master";
    var TAATitle = "Detail 1";
    var TABTitle = "Detail 2";
    var TAAATitle = "Detail 1 of Detail 1";
    var TABATitle = "Detail 1 of Detail 2";
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //Master
    Ext.define('TAModel', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3']
    });


    var TAStore = Ext.create('Ext.data.Store', {
        model: 'TAModel',
        pageSize: 5,
        remoteFilter: true,

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: '../../api/Test/post',
            extraParams: {
                data: TA
            },
            reader: {
                type: 'json',
                root: 'data',
                totalProperty: 'totalCount'
            }
        }
    });
    TAStore.load({
        params: {
            start: 0,
            limit: 5
        }
    });

    var TAQuery = Ext.widget({
        title: TATitle,
        xtype: 'form',
        layout: 'form',
        defaultType: 'textfield',
        bodyPadding: '5 5 0',
        frame: false,
        border: false,
        items: [{
            fieldLabel: 'F2',
            name: 'P0'
        }, {
            fieldLabel: 'F3',
            name: 'P1'
        }],

        buttons: [{
            text: '查詢'
        }, {
            text: '取消'
        }]
    });

    var TAForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false
        }, {
            fieldLabel: 'F2 Caption',
            name: 'F2'
        }, {
            fieldLabel: 'F3 Caption',
            name: 'F3'
        }],

        buttons: [{
            text: '儲存'
        }, {
            text: '取消', handler: hideWin1
        }]
    });

    var TATool = Ext.create('Ext.PagingToolbar', {
        store: TAStore,
        displayInfo: true,
        width: '100%',
        border: false,
        buttons: [
            { text: '新增', handler: showWin1 },
            { itemId: 'edit', text: '修編', disabled: true, handler: showWin1 },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('Confirm', '是否確定刪除?', function (btn, text) {
                        if (btn === 'yes') {
                            var s = TAGrid.getView().getSelectionModel().getSelection()[0];
                            if (s) {
                                TAStore.remove(s);
                            }
                        }
                    }
                    );
                }
            }
        ]
    });

    function getPage() {
        var f = TAQuery.getForm();
        TAStore.load({
            params: {
                start: 0,
                limit: 5
            }
        });
    }

    var pluginExpanded = true;
    var TAGrid = Ext.create('Ext.grid.Panel', {
        title: TATitle,
        store: TAStore,
        loadMask: true,

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [TATool]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: true
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100,
            sortable: true
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TAQuery.hidden === true) {
                        TAQuery.setVisible(true);
                        TAAQuery.setVisible(false);
                        TAAAQuery.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                TAGrid.down('#edit').setDisabled(records.length === 0);
                TAGrid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    TAForm.loadRecord(records[0]);
                }
            }
        }
    });

    //Detail 1
    Ext.define('TAAModel', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3']
    });

    var TAAStore = Ext.create('Ext.data.Store', {
        model: 'TAAModel',
        pageSize: 5,
        remoteFilter: true,

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: '../../api/test/post',
            extraParams: {
                data: TA
            },
            reader: {
                type: 'json',
                root: 'data',
                totalProperty: 'totalCount'
            }
        }
    });
    TAAStore.load({
        params: {
            start: 0,
            limit: 5
        }
    });

    var TAAQuery = Ext.widget({
        title: TAATitle,
        hidden: true,
        xtype: 'form',
        layout: 'form',
        defaultType: 'textfield',
        border: false,
        items: [{
            fieldLabel: 'F2',
            name: 'P0'
        }, {
            fieldLabel: 'F3',
            name: 'P1'
        }],

        buttons: [{
            text: '查詢'
        }, {
            text: '取消'
        }]
    });
    var TAAForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false
        }, {
            fieldLabel: 'F2 Caption',
            name: 'F2'
        }, {
            fieldLabel: 'F3 Caption',
            name: 'F3'
        }],

        buttons: [{
            text: '儲存'
        }, {
            text: '取消', handler: hideWin2
        }]
    });

    var TAATBar = Ext.create('Ext.PagingToolbar', {
        store: TAAStore,
        displayInfo: true,
        width: '100%',
        border: false,
        buttons: [
            { text: '新增', handler: showWin2 },
            { itemId: 'edit', text: '修編', disabled: true, handler: showWin2 },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('Confirm', '是否確定刪除?', function (btn, text) {
                        if (btn === 'yes') {
                            var s = TAAGrid.getView().getSelectionModel().getSelection()[0];
                            if (s) {
                                TAAStore.remove(s);
                            }
                        }
                    }
                    );
                }
            }
        ]
    });

    var TAAGrid = Ext.create('Ext.grid.Panel', {
        store: TAAStore,
        loadMask: true,

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [TAATBar]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: true
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100,
            sortable: true
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TAAQuery.hidden === true) {
                        TAQuery.setVisible(false);
                        TAAQuery.setVisible(true);
                        TAAAQuery.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                TAAGrid.down('#edit').setDisabled(records.length === 0);
                TAAGrid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    TAAForm.loadRecord(records[0]);
                }
            }
        }
    });

    //Detail 1 of Detail 1
    Ext.define('TAAAModel', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3']
    });

    var TAAAStore = Ext.create('Ext.data.Store', {
        model: 'TAAAModel',
        pageSize: 5,
        remoteFilter: true,

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: '../../api/Test/post',
            extraParams: {
                data: TAAA
            },
            reader: {
                type: 'json',
                root: 'data',
                totalProperty: 'totalCount'
            }
        }
    });
    TAAAStore.load({
        params: {
            start: 0,
            limit: 5
        }
    });

    var TAAAQuery = Ext.widget({
        title: TAAATitle,
        hidden: true,
        xtype: 'form',
        layout: 'form',
        defaultType: 'textfield',
        border: false,
        items: [{
            fieldLabel: 'F2',
            name: 'P0'
        }, {
            fieldLabel: 'F3',
            name: 'P1'
        }],

        buttons: [{
            text: '查詢'
        }, {
            text: '取消'
        }]
    });
    var TAAAForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false
        }, {
            fieldLabel: 'F2 Caption',
            name: 'F2'
        }, {
            fieldLabel: 'F3 Caption',
            name: 'F3'
        }],

        buttons: [{
            text: '儲存'
        }, {
            text: '取消', handler: hideWin3
        }]
    });

    var TAAATBar = Ext.create('Ext.PagingToolbar', {
        store: TAAAStore,
        displayInfo: true,
        width: '100%',
        border: false,
        buttons: [
            { text: '新增', handler: showWin3 },
            { itemId: 'edit', text: '修編', disabled: true, handler: showWin3 },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('Confirm', '是否確定刪除?', function (btn, text) {
                        if (btn === 'yes') {
                            var s = TAAAGrid.getView().getSelectionModel().getSelection()[0];
                            if (s) {
                                TAAAStore.remove(s);
                            }
                        }
                    }
                    );
                }
            }
        ]
    });

    var TAAAGrid = Ext.create('Ext.grid.Panel', {
        store: TAAAStore,
        loadMask: true,

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [TAAATBar]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: true
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100,
            sortable: true
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TAAAQuery.hidden === true) {
                        TAQuery.setVisible(false);
                        TAAQuery.setVisible(false);
                        TAAAQuery.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                TAAAGrid.down('#edit').setDisabled(records.length === 0);
                TAAAGrid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    TAAAForm.loadRecord(records[0]);
                }
            }
        }
    });
    var TAABGrid = Ext.create('Ext.grid.Panel', {
        //store: TAAAStore,
        loadMask: true,

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar'//,
            //items: [TAAATBar]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: true
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100,
            sortable: true
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TAAAQuery.hidden === true) {
                        TAQuery.setVisible(false);
                        TAAQuery.setVisible(false);
                        TAAAQuery.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                TAAAGrid.down('#edit').setDisabled(records.length === 0);
                TAAAGrid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    TAAAForm.loadRecord(records[0]);
                }
            }
        }
    });

    //Detail 2
    Ext.define('TABModel', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3']
    });

    var TABStore = Ext.create('Ext.data.Store', {
        model: 'TABModel',
        pageSize: 5,
        remoteFilter: true,

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: '../../api/Test/post',
            extraParams: {
                data: TAB
            },
            reader: {
                type: 'json',
                root: 'data',
                totalProperty: 'totalCount'
            }
        }
    });
    TABStore.load({
        params: {
            start: 0,
            limit: 5
        }
    });

    var TABQuery = Ext.widget({
        title: TABTitle,
        hidden: true,
        xtype: 'form',
        layout: 'form',
        defaultType: 'textfield',
        border: false,
        items: [{
            fieldLabel: 'F2',
            name: 'P0'
        }, {
            fieldLabel: 'F3',
            name: 'P1'
        }],

        buttons: [{
            text: '查詢'
        }, {
            text: '取消'
        }]
    });
    var TABForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false
        }, {
            fieldLabel: 'F2 Caption',
            name: 'F2'
        }, {
            fieldLabel: 'F3 Caption',
            name: 'F3'
        }],

        buttons: [{
            text: '儲存'
        }, {
            text: '取消', handler: hideWin2
        }]
    });

    var TABTBar = Ext.create('Ext.PagingToolbar', {
        store: TABStore,
        displayInfo: true,
        width: '100%',
        border: false,
        buttons: [
            { text: '新增', handler: showWinAB },
            { itemId: 'edit', text: '修編', disabled: true, handler: showWinAB },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('Confirm', '是否確定刪除?', function (btn, text) {
                        if (btn === 'yes') {
                            var s = TABGrid.getView().getSelectionModel().getSelection()[0];
                            if (s) {
                                TABStore.remove(s);
                            }
                        }
                    }
                    );
                }
            }
        ]
    });

    var TABGrid = Ext.create('Ext.grid.Panel', {
        store: TABStore,
        loadMask: true,

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [TABTBar]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: true
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100,
            sortable: true
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TABQuery.hidden === true) {
                        TAQuery.setVisible(false);
                        TAAQuery.setVisible(false);
                        TABQuery.setVisible(true);
                        TAAAQuery.setVisible(false);
                        TABAQuery.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                TABGrid.down('#edit').setDisabled(records.length === 0);
                TABGrid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    TABForm.loadRecord(records[0]);
                }
            }
        }
    });

    //Detail 1 of Detail 2
    Ext.define('TABAModel', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3']
    });

    var TABAStore = Ext.create('Ext.data.Store', {
        model: 'TABAModel',
        pageSize: 5,
        remoteFilter: true,

        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: '../../api/Test/post',
            extraParams: {
                data: TABA
            },
            reader: {
                type: 'json',
                root: 'data',
                totalProperty: 'totalCount'
            }
        }
    });
    TABAStore.load({
        params: {
            start: 0,
            limit: 5
        }
    });

    var TABAQuery = Ext.widget({
        title: TABATitle,
        hidden: true,
        xtype: 'form',
        layout: 'form',
        defaultType: 'textfield',
        border: false,
        items: [{
            fieldLabel: 'F2',
            name: 'P0'
        }, {
            fieldLabel: 'F3',
            name: 'P1'
        }],

        buttons: [{
            text: '查詢'
        }, {
            text: '取消'
        }]
    });
    var TABAForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'F1 Caption',
            name: 'F1',
            allowBlank: false
        }, {
            fieldLabel: 'F2 Caption',
            name: 'F2'
        }, {
            fieldLabel: 'F3 Caption',
            name: 'F3'
        }],

        buttons: [{
            text: '儲存'
        }, {
            text: '取消', handler: hideWinABA
        }]
    });

    var TABATBar = Ext.create('Ext.PagingToolbar', {
        store: TABAStore,
        displayInfo: true,
        width: '100%',
        border: false,
        buttons: [
            { text: '新增', handler: showWinABA },
            { itemId: 'edit', text: '修編', disabled: true, handler: showWinABA },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('Confirm', '是否確定刪除?', function (btn, text) {
                        if (btn === 'yes') {
                            var s = TAAAGrid.getView().getSelectionModel().getSelection()[0];
                            if (s) {
                                TABAStore.remove(s);
                            }
                        }
                    }
                    );
                }
            }
        ]
    });

    var TABAGrid = Ext.create('Ext.grid.Panel', {
        store: TABAStore,
        loadMask: true,

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [TABATBar]
        }],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "F1 Caption",
            dataIndex: 'F1',
            width: 100,
            sortable: true
        }, {
            text: "F2 Caption",
            dataIndex: 'F2',
            width: 100,
            sortable: true
        }, {
            text: "F3 Caption",
            dataIndex: 'F3',
            flex: 1,
            sortable: true
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (TAAAQuery.hidden === true) {
                        TAQuery.setVisible(false);
                        TAAQuery.setVisible(false);
                        TABQuery.setVisible(false);
                        TAAAQuery.setVisible(false);
                        TABAQuery.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                TABAGrid.down('#edit').setDisabled(records.length === 0);
                TABAGrid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    TABAForm.loadRecord(records[0]);
                }
            }
        }
    });

    //views
    //            var TAATabs = Ext.widget('tabpanel', {
    //                plain: true,
    //                border: false,
    //                resizeTabs: true,
    //                enableTabScroll: true,
    //                defaults: {
    //                    autoScroll: true,
    //                    closable: false
    //                },
    //                items: [{
    //                    title: TAAATitle,
    //                    items: [TAAAGrid]
    //                }]
    //            });

    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        enableTabScroll: true,
        defaults: {
            autoScroll: true
        },
        items: [{
            title: TAATitle,
            items: [//TAAGrid
            {
                layout: {
                    type: 'border',
                    padding: 0,
                    align: 'stretch'
                },
                defaults: {
                    split: true
                },
                collapsible: false,
                title: '',

                items: [{
                    region: 'north',
                    collapsible: false,
                    title: '',
                    flex: 1,
                    layout: 'fit',
                    items: [TAAGrid]
                }, {
                    region: 'center',
                    collapsible: false,
                    title: '',
                    flex: 1,
                    layout: 'fit',
                    items: []
                }]
            }
            ]
        }, {
            title: TABTitle,
            items: [TABGrid]
        }]
    });


    var viewport = Ext.create('Ext.Viewport', {
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            region: 'center',
            layout: {
                type: 'border',
                padding: 0
            },
            collapsible: false,
            title: '',
            items: [{
                region: "center",
                title: "",
                layout: "fit",
                flex: 2,
                items: [{
                    xtype: "tabpanel",
                    activeTab: 0,
                    items: [{
                        xtype: "panel",
                        title: "T1",
                        layout: "fit",
                        items: [{
                            layout: "border",
                            items: [{
                                region: "center",
                                flex: 1,
                                items: [{
                                    xtype: "tabpanel",
                                    activeTab: 0,
                                    items: [{
                                        xtype: "panel",
                                        title: "T11",
                                        items: [TAAAGrid]
                                    }, {
                                        xtype: "panel",
                                        title: "T12",
                                        items: [TAABGrid]
                                    }]
                                }]
                            }, {
                                region: "north",
                                split: true,
                                flex: 1,
                                items: [TAAGrid]
                            }]
                        }]
                    }, {
                        xtype: "panel",
                        title: "T2",
                        layout: "fit",
                        items: [{
                            layout: "border",
                            items: [{
                                region: "center",
                                layout: "fit",
                                flex: 1,
                                items: [{
                                    xtype: "tabpanel",
                                    activeTab: 0,
                                    items: [{
                                        xtype: "panel",
                                        title: "T21",
                                        items: [{
                                            xtype: "panel",
                                            title: "T21"
                                        }]
                                    }, {
                                        xtype: "panel",
                                        title: "T22",
                                        items: [{
                                            xtype: "panel",
                                            title: "T22"
                                        }]
                                    }]
                                }]
                            }, {
                                region: "north",
                                split: true,
                                flex: 1,
                                items: [TABGrid]
                            }]
                        }]
                    }]
                }]
            }, {
                region: "north",
                title: "",
                split: true,
                flex: 1,
                items: [TAGrid]
            }]

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
                items: [TAQuery, TAAQuery, TABQuery, TAAAQuery, TABAQuery]
            }
        ]
    }
        );

    var win1;
    if (!win1) {
        win1 = Ext.widget('window', {
            title: TATitle,
            closeAction: 'hide',
            width: 250,
            height: 150,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: TAForm
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

    var win2;
    if (!win2) {
        win2 = Ext.widget('window', {
            title: TAATitle,
            closeAction: 'hide',
            width: 250,
            height: 150,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: TAAForm
        });
    }
    function showWin2() {
        if (win2.hidden) { win2.show(); }
    }
    function hideWin2() {
        if (!win2.hidden) {
            win2.hide();
        }
    }

    var winAB;
    if (!winAB) {
        winAB = Ext.widget('window', {
            title: TABTitle,
            closeAction: 'hide',
            width: 250,
            height: 150,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: TABForm
        });
    }
    function showWinAB() {
        if (winAB.hidden) { winAB.show(); }
    }
    function hideWinAB() {
        if (!winAB.hidden) {
            winAB.hide();
        }
    }

    var win3;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: TAAATitle,
            closeAction: 'hide',
            width: 250,
            height: 150,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: TAAAForm
        });
    }
    function showWin3() {
        if (win3.hidden) { win3.show(); }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }

    var winABA;
    if (!winABA) {
        winABA = Ext.widget('window', {
            title: TABATitle,
            closeAction: 'hide',
            width: 250,
            height: 150,
            layout: 'fit',
            resizable: false,
            modal: true,
            items: TABAForm
        });
    }
    function showWinABA() {
        if (winABA.hidden) { winABA.show(); }
    }
    function hideWinABA() {
        if (!winABA.hidden) {
            winABA.hide();
        }
    }
});
