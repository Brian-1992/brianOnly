/* File Created: August 22, 2012 */
Ext.onReady(function () {
    var T1Get = '../../../api/UR1003/ALL';
    var T2Get = '../../../api/UR1004/GetUsersInRole';
    var T2Set = '../../../api/flow/process/TraUR100411Set';
    var T3Get = '../../../api/UR1004/GetUsersNotInRole';
    var T1Name = '群組資料';
    var T2Name = '使用者群組資料';
    var T3Name = '使用者';

    var T1Rec = 0;
    var T1LastRec = null;
    var T1RLNO = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['RLNO', 'TUSER','UNA']
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 9999,
        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T2Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['RLNO', 'RLNA', 'RLDESC']
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 60
        },

        items: [{
            fieldLabel: '群組代碼',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 10,
            width: 150,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '群組名稱',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 60,
            width: 170,
            padding: '0 4 0 4'
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

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'RLNO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                T2Store.removeAll();
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
                rootProperty: 'etts',
                totalProperty: 'rc'
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

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
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
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "群組代碼",
            dataIndex: 'RLNO',
            width: 100
        }, {
            text: "群組名稱",
            dataIndex: 'RLNA',
            width: 150
        }, {
            text: "群組說明",
            dataIndex: 'RLDESC',
            flex: 1,
            sortable: false
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //T2Grid.down('#new').setDisabled(records.length == 0);
                if (records[0]) {
                    var r = Ext.create('T2Model', { RLNO: '', TUSER: '' });
                    T2Form.loadRecord(r);
                    T2Form.getForm().findField('RLNO').setValue(records[0].get('RLNO'));
                    T1RLNO = records[0].get('RLNO');
                    T2Store.load({
                        params: {
                            p0: records[0].get('RLNO') //??
                        }
                    });
                    T3Store.load();
                }
            }
        }
    });

    //---------------------------------------------------------

    var T2Form = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        items: [{
            name: 'x'
        }, {
            name: 'TUSER'
        }, {
            name: 'RLNO'
        }, {
            name: 'UNA'
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
                    var r = form.getRecord();
                    switch (form.findField("x").getValue()) {
                        case "I":
                            var v = form.getValues();//getFieldValues(); ??
                            //r.set(v);
                            //修正重覆問題2013/09/09 思評
                            var r = Ext.create('T2Model');
                            r.set(v);

                            T2Store.insert(0, r);
                            r.commit();
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            msglabel('G0003:資料刪除成功');
                            break;
                    }
                    T3Store.load();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('錯誤', MMIS.Message.clientError);
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('錯誤', MMIS.Message.communicationError);
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('錯誤', action.result.msg);
                    }
                }
            });
        }
    }

    var T2Tool = Ext.create('Ext.toolbar.Toolbar', {
        items: [
            //{ itemId: 'new', text: '新增', disabled: true, handler: function () { } },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T2Set = '../../../api/UR1004/Delete';
                            T2Form.getForm().findField('x').setValue('D');
                            T2Submit();
                        }
                    }
                    );
                }
            }
        ],
        border: false,
        plain: true
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: T2Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T2Tool]
        }
        ],
        // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "群組代碼",
            dataIndex: 'RLNO',
            width: 80,
            sortable: false
        }, {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 80,
            sortable: true
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            flex:1,
            sortable: true
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Grid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    T2Form.loadRecord(records[0]);
                }
            }
        }


    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: ['TUSER', 'UNA', 'IDDESC']
    });

    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 40
        },
        
        items: [{
            fieldLabel: '帳號',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 20,
            width: 150,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '姓名',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 30,
            width: 150,
            padding: '0 4 0 4'
        }, {
            xtype: 'button',
            text: '查詢',
            iconCls: 'TRASearch',
            handler: T3Load
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

    // create the Data Store
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'TUSER', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1RLNO,
                    p1: T3Query.getForm().findField('P0').getValue(),
                    p2: T3Query.getForm().findField('P1').getValue()
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
            url: T3Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T3Load() {
        T3Store.load({
            params: {
                start: 0
            }
        });
    }
    //T3Load();

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add',
                text: '加入群組',
                disabled: true,
                handler: function () {
                    T2Set = '../../../api/UR1004/Create';
                    T2Form.getForm().findField('x').setValue('I');
                    T2Submit();
                }
            }
        ]
    });
    //var T3bTool = Ext.create('Ext.toolbar.Toolbar', {
    //    items: [
    //        {
    //            itemId: 'add', text: '新增', disabled: true, handler: T2Submit
    //        },
    //    ],
    //    frame: false,
    //    border: false,
    //    plain: true
    //});

    var T3Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T3Store,
        //columnLines:true,
        //disableSelection: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T3',

        dockedItems: [ {
            items: [T3Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T3Tool]
        }],

        // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "帳號",
            dataIndex: 'TUSER',
            width: 100
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 100
        }, {
            text: "人員角色說明",
            dataIndex: 'IDDESC',
            flex: 1,
            sortable: false
        }],
        listeners: {
            selectionchange: function (model, records) {
                T3Grid.down('#add').setDisabled(records.length === 0);
                if (records[0]) {
                    var f = T2Form.getForm();
                    f.findField('x').setValue('I');
                    f.findField('TUSER').setValue(records[0].get('TUSER'));
                    f.findField('UNA').setValue(records[0].get('UNA'));
                }
            }
        }
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
        items: [
        {
            //  xtype:'container',
            id: "Master",
            region: 'west',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            width: '40%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            //  xtype:'container',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            width: '20%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T2Grid]
        },
        {
            id: "SelectUsers",
            region: 'east',
            collapsible: true,
            floatable: true,
            split: true,
            width: '40%',
            minWidth: 120,
            minHeight: 140,
            title: T3Name,
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T3Grid]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
