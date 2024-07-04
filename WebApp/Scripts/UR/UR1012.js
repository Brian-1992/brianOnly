Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common',
    'WEBAPP.form.FileGridField']);

Ext.onReady(function () {
    var T1Get = '/api/UR1012/Query';
    var T1Set = '/api/UR1012/Create';
    var T1Name = "系統公佈欄維護";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['ID', 'TITLE', 'CONTENT', 'TARGET',
            { name: 'ON_DATE', type: 'date' },
            { name: 'OFF_DATE', type: 'date' }, 'VALID',
            'CREATE_BY', { name: 'CREATE_DT', type: 'date' },
            'UPDATE_BY', { name: 'UPDATE_DT', type: 'date' }, 'UPLOAD_KEY']
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
            fieldLabel: '標題',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 50,
            labelWidth: 50,
            padding: '0 4 0 4'
        }, {
            fieldLabel: '內容',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 500,
            labelWidth: 50,
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
        sorters: [{ property: 'ID', direction: 'DESC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
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
            labelAlign: 'right',
            labelWidth: 60
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'ID',
            xtype: 'hidden'
        }, {
            fieldLabel: '標題',
            name: 'TITLE',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'textarea',
            fieldLabel: '內容',
            name: 'CONTENT',
            rows: 12,
            enforceMaxLength: true,
            maxLength: 500,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
                hidden:true,
            xtype: 'radiogroup',
            name: 'TRG',
            fieldLabel: '公告對象',
            width: 250,
            items: [
                { boxLabel: '對內', width: 45, name: 'TARGET', inputValue: 'I', checked: true },
                { boxLabel: '對外', width: 45, name: 'TARGET', inputValue: 'E' },
                { boxLabel: '全部', width: 45, name: 'TARGET', inputValue: 'A' }
            ]
        }, {
            xtype: 'datefield',
            fieldLabel: '起始日期',
            format: 'Y/m/d',
            name: 'ON_DATE',
            readOnly: true,
            listeners: {
                select: {
                    fn: function () {
                        var f = T1Form.getForm();
                        var offDate = Ext.Date.add(f.findField('ON_DATE').getValue(), Ext.Date.DAY, 1);
                        f.findField('OFF_DATE').setMinValue(offDate);
                    }
                }
            }
        }, {
            xtype: 'datefield',
            fieldLabel: '結束日期',
            format: 'Y/m/d',
            name: 'OFF_DATE',
            minValue: '2018/12/26',
            readOnly: true
        }, {
            xtype: 'filegrid',
            fieldLabel: '附件',
            name: 'UPLOAD_KEY',
            width: '100%',
            height: 100
        }, {
            fieldLabel: '啟用',
            name: 'VALID',
            xtype: 'checkboxfield',
            inputValue: '1',
            uncheckedValue: '0',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '新增人員',
            name: 'CREATE_BY',
        }, {
            xtype: 'displayfield',
            fieldLabel: '新增日期',
            name: 'CREATE_DT',
            renderer: function (value, meta, record) {
                return Ext.util.Format.date(value, 'X/m/d H:i:s');
            }
        }, {
            xtype: 'displayfield',
            fieldLabel: '修改人員',
            name: 'UPDATE_BY',
        }, {
            xtype: 'displayfield',
            fieldLabel: '修改日期',
            name: 'UPDATE_DT',
            renderer: function (value, meta, record) {
                return Ext.util.Format.date(value, 'X/m/d H:i:s');
            }
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                var trg_msg = '';
                var f = T1Form.getForm();
                var trg = f.findField('TRG').getValue().TARGET;
                if (trg == 'E' || trg == 'A')
                    trg_msg = '此公告將對外開放，';

                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                if (f.isValid()) {
                    Ext.MessageBox.confirm(confirmSubmit, trg_msg + '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else
                    Ext.Msg.alert('訊息', '輸入格式不正確.');

                
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
        f.findField('TITLE').setReadOnly(true);
        f.findField('CONTENT').setReadOnly(true);
        f.findField('ON_DATE').setReadOnly(true);
        f.findField('ON_DATE').setMinValue(null);
        f.findField('OFF_DATE').setReadOnly(true);
        f.findField('OFF_DATE').setMinValue(null);
        f.findField('VALID').setReadOnly(true);
        f.findField('TRG').setReadOnly(true);
        f.findField('UPLOAD_KEY').setReadOnly(true);
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
                    T1Set = '../../../api/UR1012/Create';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/UR1012/Update';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '../../../api/UR1012/Delete';
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
            f.findField('ON_DATE').setMinValue(new Date());
            isNew = true;
            f.reset();
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            f.findField('TARGET').setValue('I');
            u = f.findField("TITLE");
        }
        else {
            var onDate = f.findField('ON_DATE').getValue();
            if (onDate != null) {
                var offDate = Ext.Date.add(onDate, Ext.Date.DAY, 1);
                f.findField('OFF_DATE').setMinValue(offDate);
            }
            u = f.findField('CONTENT');
        }
        f.findField('x').setValue(x);
        f.findField('TITLE').setReadOnly(false);
        f.findField('CONTENT').setReadOnly(false);
        f.findField('ON_DATE').setReadOnly(false);
        f.findField('OFF_DATE').setReadOnly(false);
        f.findField('VALID').setReadOnly(false);
        f.findField('TRG').setReadOnly(false);
        f.findField('UPLOAD_KEY').setReadOnly(false);
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
        },
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],

        //                // grid columns
        columns: [{
            // id assigned so we can apply custom css (e.g. .x-grid-cell-topic b { color:#333 })
            // TODO: This poses an issue in subclasses of Grid now because Headers are now Components
            // therefore the id will be registered in the ComponentManager and conflict. Need a way to
            // add additional CSS classes to the rendered cells.
            text: "標題",
            dataIndex: 'TITLE',
            width: 150
        }, {
            text: "內容",
            dataIndex: 'CONTENT',
            flex: 1
            }, {
                hidden:true,
            text: "公告對象",
            width: 70,
            align: 'center',
            renderer: function (val, meta, record) {
                switch (record.get("TARGET")) {
                    case 'I': return '<span style=\'color:blue\'>對內</span>'; break;
                    case 'E': return '<span style=\'color:green\'>對外</span>'; break;
                    case 'A': return '<span style=\'color:red\'>全部</span>'; break;
                }
            }
        }, {
                xtype: 'datecolumn',
            text: "起始日期",
            width: 90,
            dataIndex: 'ON_DATE',
            format: 'X/m/d'
            }, {
                xtype: 'datecolumn',
            text: "結束日期",
            width: 90,
            dataIndex: 'OFF_DATE',
            format: 'X/m/d'
        }, {
            text: "啟用",
            width: 70,
            align: 'center',
            renderer: function (val, meta, record) {
                switch (record.get("VALID")) {
                    case '1': return '<span style=\'color:blue\'>是</span>'; break;
                    case '0': return '<span style=\'color:red\'>否</span>'; break;
                }
            }
        }/*, {
            text: "新增人員",
            dataIndex: 'CREATE_BY',
            width: 100
        }, {
            text: "新增日期",
            dataIndex: 'CREATE_DT',
            width: 150
        }, {
            text: "修改人員",
            dataIndex: 'UPDATE_BY',
            width: 100
        }, {
            text: "修改日期",
            dataIndex: 'UPDATE_DT',
            width: 150
        }*/],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
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
        var isRowSelected = T1Grid.selModel.selected.length > 0;
        if (isRowSelected) {
            T1Grid.down('#edit').setDisabled(T1Rec === 0);
            T1Grid.down('#delete').setDisabled(T1Rec === 0);
            if (T1LastRec) {
                var f = T1Form.getForm();
                f.findField('OFF_DATE').setMinValue(null);
                isNew = false;
                T1Form.loadRecord(T1LastRec);
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
        }
        else {
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#delete').setDisabled(true);
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
            title: '修改',
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
