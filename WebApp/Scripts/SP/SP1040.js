/* File Created: August 22, 2012 */
Ext.onReady(function () {
    var T1Get = '../../../api/tree/TraSP10501Get';
    var T1Set = '../../../api/flow/process/TraSP10401Set';
    var T2Get = '../../../api/flow/process/TraSP10401GetFT';
    var T1Name = 'TableName';

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['KC', 'KV']
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T2Get,
            reader: {
                type: 'json',
                root: 'ds.T1'
            }
        },
        autoLoad: true
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
             { name: 'FG', type: 'string' },
             { name: 'PG', type: 'string' },
             { name: 'SG', type: 'string' },
             { name: 'FT', type: 'string' },
             { name: 'FS', type: 'string' },
             { name: 'F0', type: 'string' },
             { name: 'text', type: 'string' },
             { name: 'url', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.TreeStore', {
        model: 'T1Model',
        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T1Get
        },
        autoLoad: true
    });
    var T1Tool = Ext.create('Ext.toolbar.Toolbar', {
        items: [
            {
                itemId: 'append', disabled: true,
                text: '新增子項', handler: function () {
                    setFormT1('A', '新增子項');
                }
            },
            {
                itemId: 'insert', disabled: true,
                text: '新增項目於(節點)前', handler: function () {
                    setFormT1('I', '新增項目於(節點)前');
                }
            },
            {
                itemId: 'edit', text: '修編', disabled: true, handler: function () {
                    setFormT1("U", "修編");
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
        ],
        frame: false,
        border: false,
        plain: true
    });
    function setFormT1(x, t) {
        viewport.down('#tree').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "A") {
            isNew = true;
            var pg = f.findField('FG').getValue();
            var sg = f.findField('SG').getValue();
            var r = Ext.create('T1Model');
            f.loadRecord(r);
            f.findField('PG').setValue(pg);
            f.findField('SG').setValue(sg);
            var u = f.findField("FG");
            u.setReadOnly(false);
        }
        else {
            if (x === "I") {
                isNew = true;
                var pg = f.findField('PG').getValue();
                var sg = f.findField('SG').getValue();
                var fs = f.findField('FS').getValue();
                var r = Ext.create('T1Model');
                f.loadRecord(r);
                f.findField('PG').setValue(pg);
                f.findField('SG').setValue(sg);
                f.findField('FS').setValue(fs);
                var u = f.findField("FG");
                u.setReadOnly(false);
            }
            else {
                u = f.findField('FT');
            }
        }
        f.findField('x').setValue(x);
        f.findField('FT').setReadOnly(false);
        f.findField('F0').setReadOnly(false);
        f.findField('text').setReadOnly(false);
        f.findField('url').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }
    var T1Tree = Ext.create('Ext.tree.Panel', {
        id: "navtree",
        title: '',
        header: false,
        collapsible: true,
        useArrows: true,
        rootVisible: false,
        store: T1Store,
        multiSelect: false,
        singleExpand: false,
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
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


    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        id: 'simpleForm',
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
            fieldLabel: 'Id',
            name: 'FG',
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            name: 'FT',
            fieldLabel: 'Type',
            xtype: 'combobox',
            store: T2Store,
            queryMode: 'local',
            displayField: 'KV',
            valueField: 'KC',
            forceSelection: true,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: 'English',
            name: 'F0',
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: 'Chinese',
            name: 'text',
            allowBlank: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: 'URL',
            name: 'url',
            readOnly: true
        }, {
            fieldLabel: 'Parent Id',
            name: 'PG',
            xtype: 'displayfield',
            //readOnly: true,
            //fieldCls: 'readOnly',
            submitValue: true
            //hidden: true
        }, {
            fieldLabel: 'Set Id',
            name: 'SG',
            xtype: 'displayfield',
            submitValue: true
            //hidden: true
        }, {
            fieldLabel: 'Sequence',
            name: 'FS',
            xtype: 'displayfield',
            submitValue: true
            //,readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
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
                    T1Store.load();
                    myMask.hide();
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
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        viewport.down('#tree').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('FG').setReadOnly(true);
        f.findField('FT').setReadOnly(true);
        f.findField('F0').setReadOnly(true);
        f.findField('text').setReadOnly(true);
        f.findField('url').setReadOnly(true);
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }
    function setFormT1a() {
        T1Tree.down('#append').setDisabled(T1Rec === 0);
        T1Tree.down('#insert').setDisabled(T1Rec === 0);
        T1Tree.down('#edit').setDisabled(T1Rec === 0);
        T1Tree.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('FG');
            u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            T1Tree.down('#append').setDisabled(f.findField('FT').getValue() === 'L');
            T1Tree.down('#insert').setDisabled(f.findField('PG').getValue() === '');
            T1Tree.down('#delete').setDisabled(f.findField('PG').getValue() === '' || T1LastRec.childNodes.length > 0);
        }
        else {
            T1Form.getForm().reset();
        }
    }

    var viewport = Ext.create('Ext.Viewport', {
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
            width: '70%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Tree]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            split: true,
            width: '30%',
            minWidth: 120,
            minHeight: 140,
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

});
