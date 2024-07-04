Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '/api/GB0007/QueryD';
    var T1Set = '../../../api/GB0007/UpdateD';
    var T1Name = "醫院選項設定";

    var T1Rec = 0;
    var T1LastRec = null;
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'GRP_CODE', type: 'string' },
            { name: 'DATA_SEQ', type: 'string' },
            { name: 'DATA_NAME', type: 'string' },
            { name: 'DATA_VALUE', type: 'string' },
            { name: 'DATA_DESC', type: 'string' },
            { name: 'DATA_REMARK', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DATA_SEQ', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
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
            fieldLabel: '參數群組代碼',
            name: 'GRP_CODE',
            xtype: 'hidden'
        }, {
            fieldLabel: '排序',
            name: 'DATA_SEQ',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '參數名稱',
            name: 'DATA_NAME',
            readOnly: true,
            submitValue: true
        }, {
            fieldLabel: '參數值',
            name: 'DATA_VALUE',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 50,
            fieldCls: 'required',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '參數說明',
            name: 'DATA_DESC',
            readOnly: true,
            submitValue: true
        }, {
            fieldLabel: '備註',
            name: 'DATA_REMARK',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true, handler: function () {
                var f = T1Form.getForm();
                if (f.isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
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
                    case "U":
                        f2.updateRecord(r);
                        r.commit();
                        msglabel('資料修改完成');
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
        f.findField('DATA_VALUE').setReadOnly(true);
        f.findField('DATA_REMARK').setReadOnly(true);
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
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/GB0007/UpdateD';
                    setFormT1("U", '修改');
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        f.findField('x').setValue(x);
        f.findField('DATA_VALUE').setReadOnly(false);
        f.findField('DATA_REMARK').setReadOnly(false);
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
            items: [T1Tool]
        }
        ],
        columns: [{
            text: "排序",
            dataIndex: 'DATA_SEQ',
            width: 50,
            sortable: true
        }, {
            text: "參數名稱",
            dataIndex: 'DATA_NAME',
            width: 150,
            sortable: true
        }, {
            text: "參數值",
            dataIndex: 'DATA_VALUE',
            width: 250,
            sortable: true
        }, {
            text: "參數說明",
            dataIndex: 'DATA_DESC',
            width: 200,
            sortable: true
        }, {
            text: "備註",
            dataIndex: 'DATA_REMARK',
            flex: 1,
            sortable: true
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
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
        }
        else {
            T1Form.getForm().reset();
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
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '100%',
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

    T1Load();
});
