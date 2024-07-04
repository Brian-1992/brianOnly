Ext.onReady(function () {
    Ext.Loader.setConfig({
        enabled: true,
        paths: {
            'WEBAPP': '/Scripts/app'
        }
    });

    var T1Get = '/api/UR1002/All';
    var T1GetExcel = '/api/UR1002/Excel';
    var T1Set = '/api/UR1002/Change';
    var T2Get = '/api/UR1002/GetMenu';
    var encBtnGet = '/api/UR1002/GetEncBtn';
    var T1Name = '使用者';
    var T2Name = '功能';

    var T1Rec = 0;
    var T1LastRec = null;
    var IsPageLoad = true;

    // 判斷是否可用加密密碼
    function setEncBtn() {
        Ext.Ajax.request({
            url: encBtnGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            if (tb_data[i].VALUE == 'ADMG')
                                T1Tool.down('#btnEncrypt').setVisible(true); // 群組有ADMG才使用
                        }
                    }
                    
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function T1Load() {
        msglabel('訊息區：');
        T1Rec = 0;
        T1LastRec = null;
        setFormT1a();

        T1Store.loadPage(1);
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Menu').unmask();
        var f = T1Form.getForm();
        f.reset();

        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            if (f.xtype === "combo" || f.xtype === "datefield") {
                f.readOnly = true;
            }
            else {
                f.setReadOnly(true);
            }
        });

        //T1Form.down('#resetPWD').setVisible(false);
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }

    //Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var isNew = false;

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'TUSER',
            'INID',
            'UNA',
            'IDDESC',
            'EMAIL',
            'TEL',
            'EXT',
            'TITLE',
            'FAX',
            { name: 'FL', type: 'int' },
            'PA',
            'BOSS'
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10,
        //autoLoad: true,
        remoteSort: true,
        sorters: [{ property: 'TUSER', direction: 'ASC' }, { property: 'UNA', direction: 'DESC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (T2Store.getCount()) T2Store.getRootNode().removeAll();
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                }
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        defaults: {
            labelAlign: "right",
            labelWidth: 40
        },
        items: [{
            fieldLabel: '帳號',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 20,
            //padding: '0 4 0 4'
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Load();
                    }
                }
            }
        }, {
            fieldLabel: '姓名',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 30,
            //padding: '0 4 0 4'
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Load();
                    }
                }
            }
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

    var cbInid = Ext.create('WEBAPP.form.InidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        //queryParam: {
            //GRP_CODE: 'UR_INID',
            //DATA_NAME: 'INID_FLAG'
        //},
        //insertEmptyRow: true,
        editable: false,
        forceSelection: true,
        readOnly: true
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 3
        },
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90,
            labelAlign: "right"
        },

        items: [{
            fieldLabel: '帳號',
            name: 'TUSER',
            allowBlank: false,
            allowOnlyWhitespace: false,
            enforceMaxLength: true,
            maxLength: 20,
            fieldCls: 'required',
            readOnly: true,
            listeners: {
                blur: function (a, b, c) {
                    if (isNew) {
                        var f = T1Form.getForm();
                        f.findField('PA').setValue(f.findField('TUSER').getValue());
                    }
                }
            }
        },
        // cbInid,
        {
            fieldLabel: '姓名',
            name: 'UNA',
            enforceMaxLength: true,
            maxLength: 30,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            //xtype: 'xcombo',
            xtype: 'combo',
            taskFlow: 'TraUR1002GetDeptCombo',
            matchFieldWidth: false,
            listConfig: { width: 310 },
            fieldLabel: '工作單位',
            name: 'WKORG',
            readOnly: true,
            hidden: true
        }, {
            fieldLabel: 'E-Mail',
            name: 'EMAIL',
            readOnly: true
        }, {
            fieldLabel: '電話號碼',
            name: 'TEL',
            readOnly: true
        }, {
            fieldLabel: '分機號碼',
            name: 'EXT',
            readOnly: true
        }, {
            fieldLabel: '傳真',
            name: 'FAX',
            readOnly: true
        }, {
            fieldLabel: '使用者密碼',
            inputType: 'password',
            name: 'PA',
            readOnly: true,
            fieldCls: 'readOnly'
        }, {
            fieldLabel: '啟用',
            name: 'FL',
            xtype: 'checkboxfield',
            allowBlank: false,
            allowOnlyWhitespace: false,
            //fieldCls: 'required',
            readOnly: true,
            inputValue: '1',
            uncheckedValue: '0'
        }, {
            fieldLabel: '三總人員',
            name: 'BOSS',
            xtype: 'checkboxfield',
            allowBlank: false,
            allowOnlyWhitespace: false,
            readOnly: true,
            inputValue: '1',
            uncheckedValue: '0'
        }, {
            fieldLabel: '人員描述',
            xtype: 'textareafield',
            name: 'IDDESC',
            enforceMaxLength: true,
            maxLength: 50,
            width: 500,
            height: 100,
            readOnly: true,
            colspan: 3
        }, {
            itemId: 'resetPWD',
            xtype: 'button',
            text: '重設密碼',
            hidden:true,
            handler: function () {
                Ext.MessageBox.confirm('提示', '確認重設密碼？', function (btn, text) {
                    if (btn === 'yes') {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        var f = T1Form.getForm();
                        //f.findField('PWD').setValue(f.findField('TUSER').getValue());
                        var ajaxRequest = $.ajax({
                            type: "POST",
                            url: '../../../api/UR1002/ResetPassword',
                            //dataType: "json",
                            data: { TUSER: f.findField('TUSER').getValue() }
                        })
                            .done(function (data, textStatus) {
                                //alert(data);
                                debugger
                                if (data.success) {
                                    Ext.Msg.alert("提示", "密碼重置完成。");
                                }
                                else {
                                    Ext.Msg.alert("錯誤", data.msg);
                                }
                                myMask.hide();
                            })
                            .fail(function (data, textStatus) {
                                //alert("錯誤:" + data);
                                Ext.Msg.alert("錯誤", data.msg);
                                myMask.hide();
                            });
                    }
                });
            }
        }, {
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) {
                    //Added by 思評 2012/11/07
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', 'W0021:輸入資料格式有誤');//option
                    msglabel('W0021:輸入資料格式有誤');
                }
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
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                var x = f2.findField("x").getValue();
                switch (x) {
                    case "I":
                        msglabel('G0001:資料新增成功');

                        f2.findField('PA').setValue('');

                        var r = Ext.create('T1Model');
                        r.set(action.result.etts[0]);
                        T1Store.insert(0, r);
                        r.commit();

                        T1Grid.getSelectionModel().select(r);
                        T1Store.totalCount = T1Store.totalCount + 1;
                        T1Tool.onLoad();
                        T1Grid.getView().refresh();
                        break;
                    case "U":
                        msglabel('G0002:資料修改成功');

                        f2.findField('PA').setValue('');

                        var data = action.result;
                        var index = T1Store.indexOf(r);

                        //傳回來的資料有可能是null
                        var r2 = Ext.create('T1Model');
                        r2.set(action.result.etts[0]);

                        T1Store.data.items[index].data = r2.data;
                        T1Store.data.items[index].raw = r2.data;
                        r.commit();
                        T1Grid.getSelectionModel().select(r);
                        break;
                    case "D":
                        msglabel('G0003:資料刪除成功');
                        var grid1 = T1Grid.getSelectionModel();
                        var next1 = grid1.selectNext();
                        if (!next1) {
                            next1 = grid1.selectPrevious();
                        }
                        T1Store.remove(r);
                        r.commit();

                        T1Store.totalCount = T1Store.totalCount - 1;
                        T1Tool.onLoad();
                        if (next1) {
                        }
                        else {
                            if (isNaN(T1Store.totalCount) || T1Store.totalCount == 0) {
                                T1Rec = 0;
                                T1LastRec = null;
                                setFormT1a();
                            }
                            else {
                                var cp = T1Store.currentPage;
                                if (cp > 1) {
                                    T1Store.loadPage(cp - 1);
                                }
                            }
                        }
                        T1Grid.getView().refresh();
                        break;
                }
                myMask.hide();
                if (x != 'D') {
                    T1Cleanup();
                }
            },
            failure: function (form, action) {
                myMask.hide();
                switch (action.failureType) {
                    case Ext.form.action.Action.CLIENT_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                    case Ext.form.action.Action.CONNECT_FAILURE:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                    case Ext.form.action.Action.SERVER_INVALID:
                        Ext.Msg.alert('錯誤', action.result.msg);
                        break;
                }
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        //width: '100%',
        //frame: false,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '../../../api/UR1002/Create';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/UR1002/Update';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '../../../api/UR1002/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                itemId: 'btnExcel', text: '匯出Excel',
                hidden: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: 'UR1002Excel.xls' }); //檔名
                    p.push({ name: 'TS', value: 'u' }); //SQL篩選條件
                    PostForm(T1GetExcel, p);
                }
            },
            {
                itemId: 'btnEncrypt', text: '加密密碼',
                hidden: true,
                handler: function () {
                    Ext.MessageBox.confirm('提示', '是否加密所有未加密的密碼？', function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: '../../../api/UR1002/EncryptAll',
                                dataType: "json"
                            })
                                .done(function (data, textStatus) {
                                    //alert(data);
                                    Ext.Msg.alert("提示", "密碼加密完成，共加密" + data.afrs + "筆。");
                                    myMask.hide();
                                })
                                .fail(function (data, textStatus) {
                                    //alert("錯誤:" + data);
                                    Ext.Msg.alert("錯誤", data.msg);
                                    myMask.hide();
                                });
                        }
                    });
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Menu').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            f.reset();
            var r = Ext.create('T1Model');
            T1Form.loadRecord(r);
            u = f.findField("TUSER");
            u.setReadOnly(false);
            f.findField('FL').setValue('1');
        }
        else {
            u = f.findField('UNA');
            T1Form.down('#resetPWD').setVisible(true);
        }
        f.findField('x').setValue(x);
        f.findField('UNA').setReadOnly(false);
        f.findField('IDDESC').setReadOnly(false);
        f.findField('EMAIL').setReadOnly(false);
        f.findField('TEL').setReadOnly(false);
        f.findField('EXT').setReadOnly(false);
        f.findField('FAX').setReadOnly(false);
        f.findField('FL').setReadOnly(false);
        f.findField('BOSS').setReadOnly(false);

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
        uses: [
            'Ext.ux.exporter.Exporter'
        ],
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],

        columns: [{
            text: '上線',
            width: 44,
            stopSelection: false,
            xtype: 'templatecolumn',
            tpl: ['<tpl for="."><input type="checkbox" disabled="disabled" </tpl>',
                '<tpl if="FL == 1">checked="checked"</tpl>',
                '  />'
            ],
            menuDisabled: true
            //}, {
            //    text: '',
            //    dataIndex: 'FS',
            //    width: 20
        }, {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 100,
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 140
        }, {
            text: "人員描述",
            dataIndex: 'IDDESC',
            flex: 1,
            sortable: false
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //treeMask.show();
                if (records.length) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    setFormT1a();
                    T2Store.load({
                        params: {
                            p0: T1LastRec.get('TUSER'),
                            PG: null
                        },
                        callback: function () {
                        }
                    });
                    viewport.down('#form').expand();
                }
            }
        }
    });
    function setFormT1a() {
        var param0 = '';
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('TUSER');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            param0 = T1LastRec.get('TUSER');
        }
        else {
            T1Form.getForm().reset();
        }
        //T2Store.load({
        //    params: {
        //        p0: param0,
        //        PG: null
        //    },
        //    callback: function () {
        //    }
        //});
    }

    //menu
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'FG', type: 'string' },
            { name: 'PG', type: 'string' },
            { name: 'FS', type: 'string' },
            { name: 'text', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.TreeStore', {
        model: 'T2Model',
        defaultRootProperty: 'etts',
        nodeParam: 'PG',
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'Post'
            },
            url: T2Get
        },
        autoLoad: false,
        listeners: {
            beforeload: function (store, operation, options) {
                if (IsPageLoad) { return false; }
                else {
                    if (T1LastRec) {
                        operation.config.params.p0 = T1LastRec.get('TUSER');
                    }
                }
            },
            load: function (store, records, successful, eOpts) {
                treeMask.hide();
                if (!successful) {
                    Ext.Msg.alert('失敗', store.proxy.reader.rawData.msg);
                } else {
                    //T2Tree.collapseAll();
                    T2Tree.expandAll();
                    //var firstNode = T2Tree.getRootNode();
                    //if (firstNode) {
                    //    firstNode.collapse();
                    //    T2Tree.expandNode(firstNode);
                    //}
                }
            }
        }
    });
    var T2Tree = Ext.create('Ext.tree.Panel', {
        id: "navtree",
        title: '',
        header: false,
        collapsible: true,
        useArrows: true,
        rootVisible: false,
        store: T2Store,
        multiSelect: false,
        singleExpand: false,
        loadMask: true,
        cls: 'T2'
    });
    var treeMask = new Ext.LoadMask(T2Tree, { msg: '處理中...' });

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
            split: true,
            width: '60%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 't2Menu',
            region: 'east',
            layout: 'fit',
            title: T2Name,
            split: true,
            width: '30%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T2Tree]
        },
        {
            itemId: 'form',
            region: 'south',
            collapsible: true,
            floatable: true,
            split: true,
            width: '20%',
            minWidth: 120,
            minHeight: 140,
            title: '修改',
            collapsed: true,
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
    IsPageLoad = false;

    setEncBtn();
});