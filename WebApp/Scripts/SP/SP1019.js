/* File Created: August 21, 2012 */
Ext.onReady(function () {
    var T1Get = '../../../api/flow/process/TraSP10191Get';
    //var T1Get = '../../../api/sp/TwsmSP10101Get';
    var T1Exp = '../../../api/flow/export/TraSP10191Get';
    //var T1Imp = '../../../api/flow/import/TwsmSP10191Import';
    var T1Imp1 = '../../../api/flow/upload/TraSP10191SetImport';
    var T1Set = '../../../api/flow/process/TraSP10191Set';
    var T1Name = "Sample Types";
    var PAGESIZE = 20;
    var T1Rec = 0;
    var T1LastRec = null;
    var T11Data = null;
    var T12Data = null;

    //Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    function loadMask(el, flag, msg) {
        var Mask = new Ext.LoadMask(Ext.get(el), { msg: msg });
        if (flag)
            Mask.show();
        else
            Mask.hide();
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'F7']
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 60
        },
        border: false,
        items: [{
            fieldLabel: 'Field 1',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 50,
            padding: '0 4 0 4'
        }, {
            fieldLabel: 'Field 2',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 50,
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
    var T1Export = Ext.widget({
        xtype: 'form',
        defaultType: 'textfield',
        standardSubmit: true,
        url: T1Exp,
        items: [{
            name: 'sort'
        }, {
            name: 'p0'
        }, {
            name: 'p1'
        }]
    });


    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }, { property: 'F2', direction: 'ASC' }],

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
        }, {
            fieldLabel: 'F4 Caption',
            name: 'F4',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: 'F5 Caption',
            name: 'F5',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: 'F6 Caption',
            name: 'F6',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }, {
            fieldLabel: 'F7 Caption',
            name: 'F7',
            enforceMaxLength: true,
            maxLength: 50,
            readOnly: true
        }],

        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            formBind: true,     //2013-05-24: 1.由表單即時驗證將儲存按鈕致禁能
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
        f.findField('F4').setReadOnly(true);
        f.findField('F5').setReadOnly(true);
        f.findField('F6').setReadOnly(true);
        f.findField('F7').setReadOnly(true);
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
            },
            {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var frm = T1Export.getForm();
                            frm.findField('p0').setValue('%' + T1Query.getForm().findField('P0').getValue() + '%');
                            frm.findField('p1').setValue('%' + T1Query.getForm().findField('P1').getValue() + '%');
                            frm.findField('sort').setValue(T1Grid.store.sorters.keys[0]);
                            frm.submit({
                                success: function (form, action) {
                                    Ext.msg.alert('Success', action.result.msg);
                                },
                                failure: function (form, action) {
                                    Ext.Msg.alert('Failed', action.result.msg);
                                },

                                // You can put the name of your iframe here instead of _blank
                                // this parameter makes its way to Ext.form.Basic.doAction() 
                                // and further leads to creation of StandardSubmit action instance
                                /**/
                                target: '_blank'
                            }
                            );
                        }
                    });
                }
            },
            {
                itemId: 'import', text: '匯入', disabled: false,
                handler: function () {
                    win1.getLayout().setActiveItem(0);
                    showWin1();
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
        f.findField('F4').setReadOnly(false);
        f.findField('F5').setReadOnly(false);
        f.findField('F6').setReadOnly(false);
        f.findField('F7').setReadOnly(false);
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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Tool]
        }
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
            width: 100,
            sortable: false
        }, {
            text: "F4 Caption",
            dataIndex: 'F4',
            width: 100,
            sortable: false
        }, {
            text: "F5 Caption",
            dataIndex: 'F5',
            width: 100,
            sortable: false
        }, {
            text: "F6 Caption",
            dataIndex: 'F6',
            width: 100,
            sortable: false
        }, {
            text: "F7 Caption",
            dataIndex: 'F7',
            width: 100,
            sortable: false
        }
        ],
        listeners: {
            //render: function (view) {
            //    view.setLoading('Loading Grid...');
            //},
            //viewready: function (view) {
            //    view.setLoading({
            //        store: view.getStore()
            //    }).hide();
            //},
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
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('F1');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
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
            items: [T1Grid]
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

    //import

    var T1ImportForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        //standardSubmit: true,
        //url: T1Imp,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 30
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: 'Id',
            name: 'id',
            xtype: 'hidden'
        }, {
            fieldLabel: 'File',
            xtype: 'filefield',
            emptyText: '選擇一個檔案',
            name: 'SP1010',
            padding: '0 4 0 4',
            buttonText: '...',
            buttonConfig: { iconCls: 'upload-icon' }
        }]
    });
    function T1ImportSubmit() {
        //Ext.MessageBox.confirm('匯入', '是否確定匯入?', function (btn, text) {
        //if (btn === 'yes') {
        var f = T1ImportForm.getForm();
        var myMask = new Ext.LoadMask(win1, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Imp1,
            success: function (form, action) {
                myMask.hide();
                var layout = win1.getLayout();
                //active = win1.items.indexOf(layout.getActiveItem());
                layout['next']();
                Ext.getCmp('move-next').setDisabled(!layout.getNext());

                f = T1ImportForm.getForm();
                switch (f.findField('x').getValue()) {
                    case '1':
                        //$("#importContent1")[0].src = action.result.ds.S0[0].URL;
                        Ext.getCmp('move-prev').setDisabled(!layout.getPrev());
                        f.findField('id').setValue(action.result.ds.S0[0].RG);

                        T11Data = action.result.ds.T1;
                        T11Store.loadData(T11Data);
                        T11Store.load({ params: { start: 0 } });
                        break;
                    case '2':
                        Ext.getCmp('move-prev').setDisabled(true);
                        //$("#importContent2")[0].src = action.result.ds.S0[0].URL;
                        T11Data = null;
                        T12Data = action.result.ds.T1;
                        T12Store.loadData(T12Data);
                        T12Store.load({ params: { start: 0 } });
                        //T12Store.load({
                        //    params: {
                        //        start: 0
                        //    }
                        //});
                        break;
                }
                //Ext.Msg.alert('Success', action.result.msg);
                //hideWin1();
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
            }//,

            // You can put the name of your iframe here instead of _blank
            // this parameter makes its way to Ext.form.Basic.doAction() 
            // and further leads to creation of StandardSubmit action instance
            /**/
            //target: 'importContent'
        }
        );
        //}
        //});
    }

    function T1ImportCleanup() {
        hideWin1();
        T1ImportForm.getForm().reset();
    }

    var navigate = function (panel, direction) {
        // This routine could contain business logic required to manage the navigation steps.
        // It would call setActiveItem as needed, manage navigation button state, handle any
        // branching logic that might be required, handle alternate actions like cancellation
        // or finalization, etc.  A complete wizard implementation could get pretty
        // sophisticated depending on the complexity required, and should probably be
        // done as a subclass of CardLayout in a real-world implementation.
        var layout = win1.getLayout();
        if (direction === "next") {
            active = win1.items.indexOf(layout.getActiveItem());
            switch (active) {
                case 0:
                    var f = T1ImportForm.getForm();
                    f.findField('x').setValue(1);
                    T1ImportSubmit();
                    break;
                case 1:
                    T11Data = null;
                    var f = T1ImportForm.getForm();
                    f.findField('x').setValue(2);
                    T1ImportSubmit();
                    break;
                default:

            }
        }
        else {
            layout['prev']();
            Ext.getCmp('move-prev').setDisabled(!layout.getPrev());
            Ext.getCmp('move-next').setDisabled(!layout.getNext());
        }
    };

    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'F7', 'MT_']
    });

    var T11Store = Ext.create('Ext.data.ArrayStore', {
        model: 'T11Model',
        data: T11Data,
        pageSize: PAGESIZE
    });

    T11Store.on('load', function (store, records, successful, operation) {
        this.loadData(T11Data.slice((this.currentPage - 1) * PAGESIZE, (this.currentPage) * PAGESIZE));
        T11Store.totalCount = T11Data.length;
    }, T11Store);

    var T12Store = Ext.create('Ext.data.ArrayStore', {
        model: 'T11Model',
        data: T12Data,
        pageSize: PAGESIZE
    });

    T12Store.on('load', function (store, records, successful, operation) {
        this.loadData(T12Data.slice((this.currentPage - 1) * PAGESIZE, (this.currentPage) * PAGESIZE));
        T12Store.totalCount = T12Data.length;
    }, T12Store);

    var T11Tool = Ext.create('Ext.PagingToolbar', {
        store: T11Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    var T12Tool = Ext.create('Ext.PagingToolbar', {
        store: T12Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T11Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name + " Import Ready",
        //model: 'T11Model',
        store: T11Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T11Tool]
        }
        ],
        columns: [{
            text: "Status",
            dataIndex: 'MT_',
            width: 70
        }, {
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
            flex: 1
        }, {
            text: "F4 Caption",
            dataIndex: 'F4',
            width: 100,
            sortable: false
        }, {
            text: "F5 Caption",
            dataIndex: 'F5',
            width: 100,
            sortable: false
        }, {
            text: "F6 Caption",
            dataIndex: 'F6',
            width: 100,
            sortable: false
        }, {
            text: "F7 Caption",
            dataIndex: 'F7',
            width: 100,
            sortable: false
        }
        ]
    });
    var T12Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name + " Import Completed",
        store: T12Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T12Tool]
        }
        ],
        columns: [{
            text: "Status",
            dataIndex: 'MT_',
            width: 70
        }, {
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
            width: 100
        }, {
            text: "F4 Caption",
            dataIndex: 'F4',
            width: 100,
            sortable: false
        }, {
            text: "F5 Caption",
            dataIndex: 'F5',
            width: 100,
            sortable: false
        }, {
            text: "F6 Caption",
            dataIndex: 'F6',
            width: 100,
            sortable: false
        }, {
            text: "F7 Caption",
            dataIndex: 'F7',
            width: 100,
            sortable: false
        }
        ]
    }); var win1;
    if (!win1) {
        win1 = Ext.widget('window', {
            title: '匯入',
            closeAction: 'hide',
            width: 700,
            height: 500,
            layout: 'card',
            resizable: true,
            modal: true,
            bbar: [
                    {
                        id: 'move-prev',
                        text: 'Back',
                        handler: function (btn) {
                            navigate(btn.up("panel"), "prev");
                        },
                        disabled: true
                    },
                    '->', // greedy spacer so that the buttons are aligned to each side
                    {
                        id: 'move-next',
                        text: 'Next',
                        handler: function (btn) {
                            navigate(btn.up("panel"), "next");
                        }
                    }
            ],
            items: [{
                itemId: 'form',
                region: 'north',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1ImportForm]
            }, {
                itemId: 'iFrame',
                region: 'center',
                collapsible: false,
                title: '',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T11Grid]
                //html: '<iframe src="" id="importContent1" width="100%" height="100%" frameborder="0"></iframe>'
            }, {
                itemId: 'iFrame2',
                region: 'center',
                collapsible: false,
                title: '',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T12Grid]
                //html: '<iframe src="" id="importContent2" width="100%" height="100%" frameborder="0"></iframe>'
            }
            ],
            defaultFocus: T1ImportForm.getForm().findField('SP1010')
        });
    }

    function showWin1() {
        if (win1.hidden) { win1.show(); }
    }
    function hideWin1() {
        if (!win1.hidden) {
            win1.hide();
            navigate("", "prev");
            navigate("", "prev");            
        }
    }
    T1Query.getForm().findField('P0').focus();

    //form is just a container that I used to setup key-value pairs then use the form post api to send it to WebAPI or iFrame... aspx
});
