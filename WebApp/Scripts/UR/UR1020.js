Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.onReady(function () {
    var T1Get = '/api/UR1019/ALL';
    var T2Get = '/api/UR1020/GetUsersInRole';
    var T2Set = '';
    var T2Create = '/api/UR1004/Create';
    var T2Delete = '/api/UR1004/Delete';
    var T3Get = '/api/UR1020/GetUsersNotInRole';
    var T5Get = '/api/UR1020/GetRolesInUser';
    var T6Get = '/api/UR1020/GetRolesNotInUser';
    var InidComboGet = '/api/BC0002/GetInidCombo';
    var T1Name = '群組資料';
    var T2Name = '使用者群組資料';
    var T3Name = '使用者';

    var T1Rec = 0;
    var T1LastRec = null;
    var T1RLNO = '';
    var T4TUSER = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['RLNO', 'TUSER','UNA']
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',

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
            maxLength: 20,
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
        T1Store.loadPage(1);
    }

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
        
        columns: [
            { xtype: 'rownumberer' },
            {
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
                            var v = form.getValues();
                            //r.set(v);
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
                            T2Set = T2Delete;
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

        columns: [
            { xtype: 'rownumberer' },
            {
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
        fields: ['TUSER', 'UNA', 'IDDESC', 'ADUSER']
    });

    var T3InidCombo = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        labelWidth: 70,
        limit: 20,
        queryUrl: InidComboGet
    });

    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'column',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 50
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
        },
        T3InidCombo,{
            fieldLabel: 'AD帳號',
            name: 'P2',
            enforceMaxLength: true,
            maxLength: 30,
            width: 150,
            padding: '0 4 0 4'
        },
        {
            xtype: 'button',
            text: '查詢',
            handler: T3Load
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
                    p2: T3Query.getForm().findField('P1').getValue(),
                    p3: T3Query.getForm().findField('INID').getValue(),
                    p4: T3Query.getForm().findField('P2').getValue()
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
                    T2Set = T2Create;
                    T2Form.getForm().findField('x').setValue('I');
                    T2Submit();
                }
            }
        ]
    });

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
        
        columns: [
            { xtype: 'rownumberer' },
            {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 100
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 100
        }, {
            text: "AD帳號",
            dataIndex: 'ADUSER',
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

    var T4InidCombo = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        labelWidth: 70,
        limit: 20,
        queryUrl: InidComboGet
    });

    var T4Query = Ext.widget({
        xtype: 'form',
        layout: 'column',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 50
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
        },
        T4InidCombo, {
            fieldLabel: 'AD帳號',
            name: 'P2',
            enforceMaxLength: true,
            maxLength: 30,
            width: 150,
            padding: '0 4 0 4'
        },
        {
            xtype: 'button',
            text: '查詢',
            handler: T4Load
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

    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'TUSER', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p1: T4Query.getForm().findField('P0').getValue(),
                    p2: T4Query.getForm().findField('P1').getValue(),
                    p3: T4Query.getForm().findField('INID').getValue(),
                    p4: T4Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                T5Store.removeAll();
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
    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
    }

    var T4Tool = Ext.create('Ext.PagingToolbar', {
        store: T4Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T4Grid = Ext.create('Ext.grid.Panel', {
        title: T3Name,
        store: T4Store,
        //columnLines:true,
        //disableSelection: true,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T3',

        dockedItems: [{
            items: [T4Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T4Tool]
        }],
        
        columns: [
            { xtype: 'rownumberer' },
            {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 100
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 100
        }, {
            text: "AD帳號",
            dataIndex: 'ADUSER',
            width: 100
        }, {
            text: "人員角色說明",
            dataIndex: 'IDDESC',
            flex: 1,
            sortable: false
        }],
        listeners: {
            selectionchange: function (model, records) {
                if (records[0]) {
                    var r = Ext.create('T2Model', { RLNO: '', TUSER: '' });
                    T5Form.loadRecord(r);
                    T5Form.getForm().findField('TUSER').setValue(records[0].get('TUSER'));
                    T5Form.getForm().findField('UNA').setValue(records[0].get('UNA'));
                    T4TUSER = records[0].get('TUSER');
                    T5Store.load({
                        params: {
                            p0: records[0].get('TUSER')
                        }
                    });
                    T6Store.load();
                }
            }
        }
    });
    
    var T5Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'POST',
                destroy: 'POST'
            },
            url: T5Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    var T5Form = Ext.widget({
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
    function T5Submit() {
        var f = T5Form.getForm();
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
                            var v = form.getValues();
                            //r.set(v);
                            var r = Ext.create('T2Model');
                            r.set(v);

                            T5Store.insert(0, r);
                            r.commit();
                            break;
                        case "D":
                            T5Store.remove(r);
                            r.commit();
                            msglabel('G0003:資料刪除成功');
                            break;
                    }
                    T6Store.load();
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

    var T5Tool = Ext.create('Ext.toolbar.Toolbar', {
        items: [
            //{ itemId: 'new', text: '新增', disabled: true, handler: function () { } },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T2Set = T2Delete;
                            T5Form.getForm().findField('x').setValue('D');
                            T5Submit();
                        }
                    }
                    );
                }
            }
        ],
        border: false,
        plain: true
    });

    var T5Grid = Ext.create('Ext.grid.Panel', {
        title: T2Name,
        store: T5Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T5Tool]
        }
        ],
        columns: [
            { xtype: 'rownumberer' },
            {
            text: "群組代碼",
            dataIndex: 'RLNO',
            width: 80,
            sortable: false
        },{
            text: "帳號",
            dataIndex: 'TUSER',
            width: 80,
            sortable: true
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            flex: 1,
            sortable: true
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T5Grid.down('#delete').setDisabled(records.length === 0);
                if (records[0]) {
                    T5Form.loadRecord(records[0]);
                }
            }
        }


    });


    var T6Query = Ext.widget({
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
            maxLength: 20,
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
            handler: T6Load
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

    var T6Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'RLNO', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T4TUSER,
                    p1: T6Query.getForm().findField('P0').getValue(),
                    p2: T6Query.getForm().findField('P1').getValue()
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
            url: T6Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T6Load() {
        T6Store.load({
            params: {
                start: 0
            }
        });
    }

    var T6Tool = Ext.create('Ext.PagingToolbar', {
        store: T6Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add',
                text: '加入群組',
                disabled: true,
                handler: function () {
                    T2Set = T2Create;
                    T5Form.getForm().findField('x').setValue('I');
                    T5Submit();
                }
            }
        ]
    });

    var T6Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T6Store,
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T6Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T6Tool]
        }
        ],

        columns: [
            { xtype: 'rownumberer' },
            {
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
                T6Grid.down('#add').setDisabled(records.length === 0);
                if (records[0]) {
                    var f = T5Form.getForm();
                    f.findField('x').setValue('I');
                    f.findField('RLNO').setValue(records[0].get('RLNO'));
                }
            }
        }
    });


    //[頁籤]依角色(群組)管理人員
    var T1Tab = Ext.create('Ext.panel.Panel', {
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
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

    //[頁籤]依人員管理角色(群組)
    var T2Tab = Ext.create('Ext.panel.Panel', {
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                region: 'west',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                width: '40%',
                minWidth: 50,
                minHeight: 140,
                border: false,
                items: [T4Grid]
            },
            {
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                width: '20%',
                minWidth: 50,
                minHeight: 140,
                border: false,
                items: [T5Grid]
            },
            {
                region: 'east',
                collapsible: true,
                floatable: true,
                split: true,
                width: '40%',
                minWidth: 120,
                minHeight: 140,
                title: T1Name,
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T6Grid]
            }
        ]
    });

    var tabs = Ext.widget('tabpanel', {
        plain: true,
        resizeTabs: true,
        deferredRender: false,
        cls: 'tabpanel',
        defaults: {
            layout: 'fit',
        },
        items: [{
            id: 'tabEditByRole',
            title: '依角色(群組)管理人員',
            items: [T1Tab],
            closable: false
        }, {
            id: 'tabEditByUser',
            title: '依人員管理角色(群組)',
            items: [T2Tab],
            closable: false
        }],
        listeners: {
            tabchange: function (tabPanel, newCard, oldCard, eOpts) {
                if (oldCard) {
                    oldCard.tab.btnInnerEl.setStyle('color', '#4a74a8');
                }
                if (newCard) {
                    newCard.tab.btnInnerEl.setStyle('color', 'brown');
                }
            },
            boxready: function (tabPanel) {
                tabPanel.fireEvent('tabchange', tabPanel, tabPanel.activeTab, null, null);
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
        items: [{
            itemId: 'tree',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            width: '100%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [tabs]
        }]
    });

    T1Query.getForm().findField('P0').focus();
});
