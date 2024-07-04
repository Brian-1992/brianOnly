Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '/api/CD0012/Query';
    var T1Set = '/api/CD0012/Create';
    var queryInidComboGet = '/api/CD0012/GetQueryInidCombo';
    var formInidComboGet = '/api/CD0012/GetFormInidCombo';
    var GetInidNameByInid = '/api/CD0012/GetInidNameByInid';
    var T1Name = "中央庫房複雜度"; // 中央庫房複雜度維護

    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['INID', 'INID_NAME', 'COMPLEXITY']
    });

    var inidQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    function setInidCombo() {
        Ext.Ajax.request({
            url: queryInidComboGet,
            method: reqVal_p,
            success: function (response) {
                inidQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            inidQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setInidCombo();

    var T1FormInid = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心代碼',
        limit: 20,
        queryUrl: formInidComboGet,
        //storeAutoLoad: true,
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        listeners: {
            select: function (field, record, eOpts) {
                if (field.getValue() != '' && field.getValue() != null && field.readOnly == false)
                    chkInid(field.getValue());
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80,
            labelAlign: 'right'
        },
        border: false,
        items: [{
            xtype: 'combo',
            store: inidQueryStore,
            displayField: 'COMBITEM',
            valueField: 'VALUE',
            id: 'P0',
            name: 'P0',
            margin: '1 0 1 0',
            fieldLabel: '責任中心代碼',
            anyMatch: true,
            queryMode: 'local',
            width: 350,
            autoSelect: true,
            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
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
        sorters: [{ property: 'INID', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function () { setFormT1a(); }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST', // by default GET
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
        T1Tool.moveFirst();
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
            labelAlign: 'right',
            labelWidth: 60
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, T1FormInid, {
            xtype: 'displayfield',
            fieldLabel: '責任中心名稱',
            name: 'INID_NAME',
            readOnly: true
        }, {
            fieldLabel: '複雜度',
            name: 'COMPLEXITY',
            enforceMaxLength: true,
            maxLength: 4,
            readOnly: true,
            maskRe: /[0-9.]/,
            regexText: '只能輸入最多整數位兩位,小數一位且大於0的數字',
            regex: /^(([1-9]\d?(\.\d)?)|(0\.[1-9]))$/,
            allowBlank: false,
            fieldCls: 'required'
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                if (T1Form.getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else
                    Ext.Msg.alert('訊息', '輸入格式不正確');
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
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
                        var v = action.result.etts[0];
                        r.set(v);
                        T1Store.insert(0, r);
                        r.commit();
                        break;
                    case "U":
                        var v = action.result.etts[0];
                        r.set(v);
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
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('INID').setReadOnly(true);
        f.findField('COMPLEXITY').setReadOnly(true);
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
                    T1Set = '/api/CD0012/Create';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/CD0012/Update';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/CD0012/Delete';
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
            f.reset();
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            u = f.findField("INID");
            f.findField('INID').setReadOnly(false);
        }
        else {
            u = f.findField('COMPLEXITY');
        }
        f.findField('x').setValue(x);
        f.findField('COMPLEXITY').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
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
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [{
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "責任中心名稱",
            dataIndex: 'INID_NAME',
            width: 200
        }, {
            text: "複雜度",
            dataIndex: 'COMPLEXITY',
            width: 90
        }, {
            sortable: false,
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        var isRowSelected = T1Grid.selModel.selected.length > 0;
        if (isRowSelected) {
            T1Grid.down('#edit').setDisabled(T1Rec === 0);
            T1Grid.down('#delete').setDisabled(T1Rec === 0);
            if (T1LastRec) {
                var f = T1Form.getForm();
                T1Form.loadRecord(T1LastRec);
                f.findField('x').setValue('U');
            }
            else {
                T1Form.getForm().reset();
            }
        }
        else {
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#delete').setDisabled(true);
        }
    }

    // 依責任中心代碼取得責任中心名稱
    function chkInid(parInid) {
        Ext.Ajax.request({
            url: GetInidNameByInid,
            method: reqVal_p,
            params: { inid: parInid },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        T1Form.getForm().findField('INID_NAME').setValue(tb_data[0].INID_NAME); // 填入T1Form的責任中心名稱
                    }
                    else {
                        Ext.Msg.alert('訊息', '責任中心代碼不存在,請重新輸入!');
                        T1Form.getForm().findField('INID_NAME').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
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
            items: [{
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
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        items: [T1Grid]
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
