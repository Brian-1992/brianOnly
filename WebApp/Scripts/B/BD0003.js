Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

//Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T11Set = '';
    var GetInidByTuser = '/api/BC0002/GetInidByTuser';
    var MATComboGet = '../../../api/BA0002/GetMATCombo';
    var T1Name = "衛材訂單備註";

    var T1Rec = 0;
    var T1LastRec = null;
    var T11Rec = 0;
    var T11LastRec = null;
    var inidc;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    // 物品類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('P2').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var mLabelWidth = 60;
    var mWidth = 180;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P2',
                    //enforceMaxLength: true,
                    labelWidth: 70,
                    width: 220,
                    padding: '0 4 0 4',
                    store: MATQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                //{
                //    xtype: 'combobox',
                //    name: 'P0',
                //    fieldLabel: '合約種類',
                //    displayField: 'TEXT',
                //    valueField: 'VALUE',
                //    queryMode: 'local',
                //    autoSelect: true,
                //    multiSelect: false,
                //    editable: false, typeAhead: true, forceSelection: true,
                //    matchFieldWidth: true,
                //    submitValue: true,
                //    allowBlank: false,
                //    store: Ext.create('Ext.data.Store', {
                //        fields: ['TEXT', 'VALUE'],
                //        data: [
                //            { TEXT: '全部', VALUE: '' },
                //            { TEXT: '合約', VALUE: '0' },
                //            { TEXT: '非合約', VALUE: '2' },
                //            { TEXT: '小採', VALUE: '3' }
                //        ]
                //    }),
                //    value:''
                //},
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        //getINID(parent.parent.userId);
                        T1Load();
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [' M_CONTID', 'TP', 'MEMO ', 'SMEMO ', 'CREATE_TIME', 'CREATE_USER', 'UPDATE_TIME', 'UPDATE_USER', 'UPDATE_IP', 'INID', 'DLINE_DT', 'MAT_CLASS']

    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        //pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'M_CONTID', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    //p0: T1Query.getForm().findField('P0').getValue(),
                    p1: session['Inid'],
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BD0003/GetPH_MAILSP',
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




    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/BD0003/MasterCreateBD';
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 't1edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BD0003/MasterUpdateBD';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/BD0003/MasterDeleteBD';
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
        // viewport.down('#t11Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        //alert(parent.parent.userId);
        if (x === "I") {
            isNew = true;
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            f.findField('INID').setValue(session['Inid']);
            f.findField('MAT_CLASS').setValue("");
            f.findField('DLINE_DT').setValue("");
            u = f.findField('INID');
        }
        else {
            u = f.findField('INID');
        }
        f.findField('x').setValue(x);
        f.findField('M_CONTID').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);
        f.findField('SMEMO').setReadOnly(false);
        f.findField('DLINE_DT').setReadOnly(false);
        f.findField('MAT_CLASS').setReadOnly(false);
        f.findField('INID').setReadOnly(true);
        f.findField('INID').setVisible(true);
        T1Form.down('#t1cancel').setVisible(true);
        T1Form.down('#t1submit').setVisible(true);
        u.focus();

    }


    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "物料類別",
            dataIndex: 'MAT_CLASS',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "合約種類",
            dataIndex: 'M_CONTID',
            style: 'text-align:left',
            align: 'left',
            width: 100,
            renderer: function (value, metaData, record, rowIndex) {
                if (value == "0")
                    return value + '-合約';
                else if (value == "2")
                    return value + '-非合約';
                else if (value == "3")
                    return value + '-小採';
                else
                    return value;
            }
        }, {
            text: "MAIL備註",
            dataIndex: 'MEMO',
            style: 'text-align:left',
            align: 'left',
            width: 400
        }, {
            text: "交貨起始日期",
            dataIndex: 'DLINE_DT',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "特殊備註",
            dataIndex: 'SMEMO',
            style: 'text-align:left',
            align: 'left',
            width: 400
        }, {
            text: "責任中心",
            dataIndex: 'INID',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
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
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
            }
        }
    });
    function setFormT1a() {

        T1Grid.down('#t1edit').setDisabled(T1Rec === 0);
        T1Grid.down('#t1delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('M_CONTID').setValue({ M_CONTID: T1LastRec.data.M_CONTID });
            f.findField('x').setValue('U');
            
            // 非新增/剔退的資料就不允許修改/刪除
            T1Grid.down('#t1edit').setDisabled(false);
            T1Grid.down('#t1delete').setDisabled(false);
        }
        else {
            T1Form.getForm().reset();
        }
    }

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: { type: 'table', columns: 1 },

        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        },  {
            xtype: 'combo',
            fieldLabel: '物料類別',
            name: 'MAT_CLASS',
            enforceMaxLength: true,
            readOnly: true,
            store: MATQueryStore,
            displayField: 'TEXT',
            valueField: 'VALUE',
            allowBlank: false,
            queryMode: 'local',
            anyMatch: true,
            fieldCls: 'required',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
        },  {
            xtype: 'radiogroup',
            fieldLabel: '合約種類',
            name: 'M_CONTID',
            vertical: true,
            width: 280,
            items: [{
                boxLabel: '合約',
                name: 'M_CONTID',
                inputValue: '0'
            }, {
                boxLabel: '非合約',
                name: 'M_CONTID',
                inputValue: '2'
            }, {
                boxLabel: '小採',
                name: 'M_CONTID',
                inputValue: '3'
            }]
        }, {
            xtype: 'textareafield',
            fieldLabel: 'MAIL備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 4000,
            height: 250,
            width: "100%",
            readOnly: true
        }, {
            xtype: 'textareafield',
            fieldLabel: '特殊備註',
            name: 'SMEMO',
            enforceMaxLength: true,
            maxLength: 4000,
            height: 250,
            width: "100%",
            readOnly: true
        }, {
            xtype: 'datefield',
            fieldLabel: '交貨起始日期',
            name: 'DLINE_DT',
            enforceMaxLength: true,
            maxLength: 100,
            // hidden: true,
            readOnly: true
        }, {
            fieldLabel: '責任中心代碼',
            name: 'INID',
            enforceMaxLength: true,
            maxLength: 100,
            hidden: true,
            readOnly: true

        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();

                        }
                    }
                    );
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                            
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.M_CONTID);
                            T1Query.getForm().findField('P2').setValue(v.MAT_CLASS);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r);
                            r.commit();
                            msglabel('訊息區:資料刪除成功');
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
        //  viewport.down('#t11Grid').unmask();
        var f = T1Form.getForm();
        //f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype == "datefield") {
        //        fc.readOnly = true;
        //    }
        //});
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
    }



    function getINID(parTuser) {
        Ext.Ajax.request({
            url: GetInidByTuser,
            method: reqVal_p,
            params: { tuser: parTuser },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {

                        inidc = tb_data[0].INID
                        //setComboData(inidc);
                        T1Form.getForm().findField('INID').setValue(tb_data[0].INID);
                        //T1Form.getForm().findField('INIDNAME').setValue(tb_data[0].INID_NAME);
                    }
                    else {
                        //T1Form.getForm().findField('APP_INID').setValue('');
                        T1Form.getForm().findField('INID').setValue('');
                        //T1Form.getForm().findField('INIDNAME').setValue('');
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
        items: [
            {
                itemId: 'tGrid',
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
                            itemId: 't1Grid',
                            region: 'center',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            border: false,
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
                width: 350,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }
        ]
    });
    //ComboData();
    // getINID(session['UserId']);
    //alert(session['Inid'] );
    //T1Query.getForm().findField('P0').focus();
});
