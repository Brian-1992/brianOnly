Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common',
    'WEBAPP.form.ImageGridField',
    'WEBAPP.form.FileGridField',
    'WEBAPP.form.FileButtonField']);

/* File Created: August 22, 2012 */
Ext.onReady(function () {
    var T1Get = '/api/UR1005/All';
    var T1Append = '/api/UR1005/Append'; //新增子項
    var T1Insert = '/api/UR1005/Insert'; //新增於節點前
    var T1Update = '/api/UR1005/Update'; //修改
    var T1Delete = '/api/UR1005/Delete'; //刪除
    var T1Set = T1Update;
    var T1UpdateFS = '/api/UR1005/UpdateFS'; //修改
    var T2Get = '../../../api/GetAccessTime/AccessDetail';
    var T1Name = '';

    var T1Rec = 0;
    var T1LastRec = null;
    var selectNode;
    var IsPageLoad = true;
    var doRefresh = false;


    if (!-[1, ]) {
        alert("IE8以下瀏覽器無法使用此功能頁面!建議使用IE9以上或Chrome、Firefox瀏覽器");
        history.go(-1);
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
             { name: 'FG', type: 'string' },
             { name: 'PG', type: 'string' },
             { name: 'SG', type: 'string' },
             { name: 'FT', type: 'string' },
             { name: 'FS', type: 'string' },
             { name: 'F0', type: 'string' },
             { name: 'FL', type: 'int' },
             { name: 'F1', type: 'string' },
             { name: 'FA0', type: 'string' },
             { name: 'FU', type: 'string' },
             { name: 'FUP', type: 'string' },
             { name: 'FD', type: 'string' },
             { name: 'AN', type: 'string' },    //Attach name
             { name: 'MFG', type: 'string' },
             { name: 'MFS', type: 'string' },
             { name: 'ATTACH_URL', type: 'string' }
        ]
    });

    function getToolbar()
    {
        return Ext.create('Ext.toolbar.Toolbar', {
            items: [
                {
                    text: '重新排序選單',
                    handler: function () {
                        Ext.MessageBox.confirm('警告', '是否確定重新排序(此動作將會重新排序功能選單序號)?', function (btn, text) {
                            if (btn === 'yes') {
                                doRefresh = true;
                                T1Load();
                            }
                        });
                    }
                }, {
                    itemId: 'append', disabled: true,
                    text: '新增子項', handler: function () {
                        T1Set = T1Append;
                        setFormT1('A', '新增子項');
                    }
                },
                {
                    itemId: 'insert', disabled: true,
                    text: '新增項目於(節點)前', handler: function () {
                        T1Set = T1Insert;
                        setFormT1('I', '新增項目於(節點)前');
                    }
                },
                {
                    itemId: 'edit', text: '修改', disabled: true, handler: function () {
                        T1Set = T1Update;
                        setFormT1("U", "修改");
                    }
                },
                {
                    itemId: 'delete', text: '刪除', disabled: true,
                    handler: function () {
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = T1Delete;
                                T1Form.getForm().findField('x').setValue('D');
                                T1Submit();
                            }
                        }
                        );
                    }
                },
                {
                    labelAlign: 'right',
                    labelWidth: 100,
                    width: 250,
                    xtype: 'triggerfield',
                    fieldLabel: '編號或名稱篩選',
                    triggerCls: 'x-form-clear-trigger',
                    onTriggerClick: function () {
                        var store = this.up('treepanel').store;

                        this.reset();
                        store.clearFilter();
                        this.focus();
                    },
                    listeners: {
                        change: function () {
                            var tree = this.up('treepanel'),
                                v,
                                matches = 0;
                            try {
                                v = new RegExp(this.getValue(), 'i');
                                tree.store.clearFilter();
                                tree.store.filter({
                                    filterFn: function (node) {
                                        if (!node) return;

                                        var children = node.childNodes,
                                            len = children && children.length,
                                            visible = node.isLeaf() ? v.test(node.get('FG')) || v.test(node.get('F1')) : true,
                                            i;
                                        
                                        //for (i = 0; i < len && !(visible = children[i].get('visible')); i++);
                                        //for (i = 0; i < len && !(visible = v.test(children[i].get('FG'))); i++);
                                        
                                        //alert(node.get('FG') + '的child有:' + len + '個');

                                        return visible;
                                    }
                                });
                                //tree.down('#matches').setValue(matches);
                            } catch (e) {
                                
                                this.markInvalid('Invalid regular expression');
                            }
                        },
                        buffer: 250
                    }
                }
            ],
            frame: false,
            border: false,
            plain: true
        });
    }
    var T1Tool = getToolbar();


    function setFormT1(x, t) {
        var pg, sg, r, u;
        viewport.down('#tree').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "A") {
            //isNew = true;
            pg = f.findField('FG').getValue();
            sg = f.findField('SG').getValue();
            r = Ext.create('T1Model');
            f.loadRecord(r);
            f.findField('PG').setValue(pg);
            f.findField('SG').setValue(sg);
            u = f.findField("FG");
            u.setReadOnly(false);
        } else {
            if (x === "I") {
                //isNew = true;
                pg = f.findField('PG').getValue();
                sg = f.findField('SG').getValue();
                var fs = f.findField('FS').getValue();
                r = Ext.create('T1Model');
                f.loadRecord(r);
                f.findField('PG').setValue(pg);
                f.findField('SG').setValue(sg);
                f.findField('FS').setValue(fs);
                u = f.findField("FG");
                u.setReadOnly(false);
            } else {
                u = f.findField('FT');
                f.findField('ATTACH_URL').setReadOnly(false);
            }
        }
        f.findField('x').setValue(x);
        f.findField('FT').setReadOnly(false);
        f.findField('F0').setReadOnly(false);
        f.findField('F1').setReadOnly(false);
        f.findField('FA0').setReadOnly(false);
        f.findField('FU').setReadOnly(false);
        f.findField('FUP').setReadOnly(false);
        f.findField('FD').setReadOnly(false);
        f.findField('FL').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    function getTreeStore() {
        return Ext.create('Ext.data.TreeStore', {
            model: 'T1Model',
            defaultRootProperty: 'etts',
            nodeParam: 'PG',
            proxy: {
                type: 'ajax',
                actionMethods: {
                    create: 'POST',
                    read: 'POST',
                    update: 'POST',
                    destroy: 'POST'
                },
                url: T1Get
            },
            listeners: {
                beforeload: function (store, operation, options) {
                    if (IsPageLoad) { return false; }
                },
                load: function (store, records, success, eOpts) {
                    myMask.hide();
                    if (success) { store.tree.expandAll(); }
                }
            }
        });
    }
    var T1Store = getTreeStore();

    var getTargetFG = function (dropPosition, overModel) {
        if (dropPosition === 'before') {
            return overModel.get('FG');
        } else if (dropPosition === 'after') {
            return overModel.nextSibling ? overModel.nextSibling.get('FG') : null;
        }
        return null;
    };

    var getTargetPG = function (dropPosition, overModel) {
        if (dropPosition === 'append') {
            return overModel.get('FG');
        }
        return overModel.parentNode.get('FG');
    };
    function doAppend() {
        if (T1LastRec !== null) {
            var f = T1Form.getForm();

            myMask.show();
            f.submit({
                url: T1Set,

                success: function (form, action) {
                    try {
                        //var row = action.result.ds.T1[0];
                        var row = action.result.etts[0];
                        var pNode = selectNode;
                        var childNode = pNode.insertChild(row.FS, row);
                        childNode.commit();
                        T1Tree.getSelectionModel().select(childNode);
                        pNode.expand();
                        selectNode = childNode;
                    } catch (e) { }
                    myMask.hide();
                    T1Cleanup();
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
                            break;
                    }
                }
            });
        }
    }
    function doInsert() {
        if (T1LastRec !== null) {
            var f = T1Form.getForm();

            myMask.show();
            f.submit({
                url: T1Set,

                success: function (form, action) {
                    try {
                        //var row = action.result.ds.T1[0];
                        var row = action.result.etts[0];
                        var pNode = selectNode.parentNode;
                        var sq = row.FS;
                        var childNode = pNode.insertChild(sq, row);
                        T1Tree.getSelectionModel().select(childNode);
                        pNode.expand();
                        selectNode = childNode;
                        /* 不在client端增加FS
                        var n = childNode.nextSibling;
                        sq++;
                        while (n !== null) {
                            n.data.FS = sq;
                            n.commit();
                            sq++;
                            n = n.nextSibling;
                        }*/
                    } catch (e) {
                        alert(e.message);
                    }
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
                            break;
                    }
                }
            });
        }
    }

    var modifyMayUpdateNode = function (dropPosition, node, overModel, dropHandlers) {
        //console.log("Select:" + node.data.FG);
        var i = node.data.FS;
        var n = node.nextSibling;
        while (n !== null) {
            n.data.FS = i;
            n.commit();
            i++;
            n = n.nextSibling;
        }

        dropHandlers.processDrop();

        switch (dropPosition) {
            case "append":
                //console.log("Append:" + overModel.data.FG);
                node.data.FS = overModel.childNodes.length;
                node.commit();
                break;
            case "before":
                //console.log("Before:" + overModel.data.FG);
                n = overModel;
                i = n.data.FS;
                node.data.FS = i;
                node.commit();
                i++;
                while (n !== null) {
                    n.data.FS = i;
                    n.commit();
                    i++;
                    n = n.nextSibling;
                }
                break;
            case "after":
                //console.log("After:" + overModel.data.FG);
                i = overModel.data.FS;
                i++;
                node.data.FS = i;
                node.commit();
                n = overModel.nextSibling;
                if (n !== null) {
                    i = n.data.FS;
                    i++;
                    while (n !== null) {
                        n.data.FS = i;
                        n.commit();
                        i++;
                        n = n.nextSibling;
                    }
                }
                break;
            default:
                alert("Unhandled: " + dropPosition);
        }
    };


    function getCellEditor() {
        return Ext.create('Ext.grid.plugin.CellEditing', {
            clicksToEdit: 1,
            autoCancel: false,
            listeners: {
                edit: function (editor, context, eOpts) {
                    Ext.Ajax.request({
                        url: T1UpdateFS,
                        method: reqVal_p,
                        params: {
                            FG: context.record.data['FG'],
                            FS: context.record.data['FS']
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel("排序已儲存");
                            }
                        },
                        failure: function (response) {
                            msglabel("排序儲存發生例外錯誤");
                        }
                    });
                },
                canceledit: function (editor, context, eOpts) {
                }
            }
        });
    }

    function getTreePanel(toolbar, store)
    {
        return Ext.create('Ext.tree.Panel', {
            id: "navtree",
            title: '',
            header: false,
            collapsible: true,
            useArrows: true,
            rootVisible: false,
            store: store,
            multiSelect: false,
            singleExpand: false,
            overflowY: 'scroll',
            scrollable: true,
            cls: 'T1',
            plugins: [
                getCellEditor()
            ],
            
            viewConfig: {
                plugins: {
                    ptype: 'treeviewdragdrop'
                },
                listeners: {
                    beforedrop: function (node, data, overModel, dropPosition, dropHandlers, eOpts) {
                        var record = data.records[0],
                            targetFG = getTargetFG(dropPosition, overModel),
                            targetPG = getTargetPG(dropPosition, overModel),
                            params = {
                                x: 'MoveBefore',
                                moveFG: record.get('FG'),
                                movePG: record.get('PG'),
                                moveFS: record.get('FS'),
                                targetFG: targetFG,
                                targetPG: targetPG
                            };
                        myMask.show();
                        dropHandlers.wait = true;
                        Ext.Ajax.request({
                            url: T1Set2,
                            params: params,
                            success: function (response) {
                                var rep = Ext.decode(response.responseText);
                                if (rep.success) {
                                    //modifyMayUpdateNode(node, record.parentNode, targetFG ? overModel.parentNode : overModel);
                                    //console.log("data: " + data.records[0].data.FG + ",overModel: " + overModel.data.FG);
                                    modifyMayUpdateNode(dropPosition, data.records[0], overModel, dropHandlers);

                                } else {
                                    Ext.Msg.alert('錯誤', rep.msg);
                                    dropHandlers.cancelDrop();
                                }
                                myMask.hide();
                            },
                            failure: function (response, opts) {
                                dropHandlers.cancelDrop();
                                Ext.Msg.alert('錯誤', response.statusText);
                                myMask.hide();
                            }
                        });
                    },
                    itemclick: function (sender, records, item, index, e, eOpts) {
                        var fg = records.data.MFG;
                        var attach = records.data.AN;
                        var path = 'DOC\\' + records.data.FG.substring(0, 2);
                        var FA0 = '';
                        if (fg === '') {
                            FA0 = '<span style="color:#FF1C19">尚未上傳文件</span>';
                        } else {
                            FA0 = '<a href=javascript:getFileName("' + fg + '");>' + attach + '<img src="../../../Images/TRA/save.gif" align="absmiddle" /></a>';
                        }

                        T1Form.getForm().findField('ATTACH_File').setValue(FA0);
                        T1Form.getForm().findField('Path').setValue(path);
                    }
                }
            },
            
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                items: [toolbar]
            }
            ],
            columns: [{
                text: '編號',
                dataIndex: 'FG',
                width: 70
            }, {
                text: '排序',
                dataIndex: 'FS',
                align: 'right',
                width: 46,
                menuDisabled: true,
                editor: {
                    xtype: 'numberfield',
                    valueAlign: 'right'
                }
            }, {
                text: '上線',
                width: 44,
                align: 'center',
                renderer: function (val, meta, record) {
                    var _fl = record.get('FL');
                    switch (_fl)
                    {
                        case 0: return '<span style="color:red">否</span>';
                        case 1: return '是';
                        default: return _fl;
                    }
                },
                menuDisabled: true,
                allowSorting: false
            }, {
                xtype: 'treecolumn',
                dataIndex: 'F1',
                flex: 1
            }, {
                text: 'Controller路徑',
                renderer: function (val, meta, record) {
                    var _fa0 = record.get('FA0');
                    var _fg = record.get('FG');
                    var _style = 'style="color:darkgreen";';
                    if (_fa0 == '/Form/Index') _style = 'style="color:red";';
                    if (_fa0 && _fa0.length > 0) _fa0 = '<span ' + _style + '>' + _fa0 + '</span>/' + _fg;
                    var _fup = record.get('FUP');
                    if (_fup && _fup.length > 0) _fup = '?<span style="color: blue;">' + _fup + '</span>';
                    return _fa0 + _fup;
                },
                flex: 1
            }, {
                text: 'JS路徑',
                renderer: function (val, meta, record) {
                    var _fu = record.get('FU');
                    if (_fu && _fu.length > 0) _fu = '/Scripts/<span style="color:blue;">' + _fu + '</span>.js';
                    //var _fup = record.get('FUP');
                    //if (_fup && _fup.length > 0) _fup = '?<span style="color: blue;">' + _fup + '</span>';
                    //return _fu + _fup;
                    return _fu;
                },
                flex: 1
            }],

            listeners: {
                load: function (store, records, success) {
                    /*
                    try {
                        selectNode = T1Tree.getRootNode().childNodes[0];
                        T1Tree.getSelectionModel().select(selectNode);
                    } catch (e) {
                    }*/
                },
                selectionchange: function (model, records) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    setFormT1a();
                },
                itemclick: function (view, node) {
                    selectNode = node;

                    return;
                    var f = T1Form.getForm();
                    Ext.Ajax.request({
                        url: T2Get,
                        params: {
                            fg: selectNode.data.FG
                        },
                        method: reqVal_p,
                        success: function (response) {
                            responseText = Ext.decode(response.responseText);
                            if (responseText.success) {
                                f.findField("modify").setValue(responseText.msg);
                            } else {
                                f.findField("modify").setValue(responseText.msg);
                            }
                        },
                        failure: function (response, options) {
                            f.findField("modify").setValue(responseText.msg);
                        },
                        callback: function () {

                        }
                    });
                }
            }

        });
    }
    var T1Tree = getTreePanel(T1Tool, T1Store); T1Store.tree = T1Tree;

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        id: 'simpleForm',
        frame: false,
        autoScroll: true,
        cls: 'T1b',
        bodyPadding: '5 5 0',
        border: false,
        defaultType: 'textfield',

        fieldDefaults: {
            msgTarget: 'side',
            labelAlign: "right",
            labelWidth: 110
        },

        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '群組編號',
            name: 'SG',
            xtype: 'displayfield',
            submitValue: true
            //hidden: true
        }, {
            fieldLabel: '父編號',
            name: 'PG',
            xtype: 'displayfield',
            //readOnly: true,
            //fieldCls: 'readOnly',
            submitValue: true
            //hidden: true
        }, {
            fieldLabel: ' 排序',
            name: 'FS',
            xtype: 'displayfield',
            submitValue: true
            //,readOnly: true
        }, {
            fieldLabel: '編號',
            name: 'FG',
            maxLength: 20,
            enforceMaxLength: true,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '型態',
            name: 'FT',
            xtype: 'combobox',
            store: Ext.create('Ext.data.Store', {
                fields: ['KC', 'KV'],
                data: [{
                    'KC': 'F',
                    'KV': '資料夾'
                }, {
                    'KC': 'L',
                    'KV': '表單'
                }]
            }),
            queryMode: 'local',
            displayField: 'KV',
            valueField: 'KC',
            editable: false,
            forceSelection: true,
            fieldCls: 'required',
            allowBlank: false,
            allowOnlyWhitespace: false,
            value: 'F',
            readOnly: true
        }, {
            fieldLabel: '程式編碼',
            name: 'F0',
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '中文名稱',
            name: 'F1',
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '執行環境',
            name: 'FA0',
            xtype: 'combobox',
            store: Ext.create('Ext.data.Store', {
                fields: ['VL', 'TX'],
                data: [{
                    'VL': '',
                    'TX': ''
                }, {
                    'VL': '/Form/Index',
                    'TX': '一般顯示設定'
                }, {
                    'VL': '/Form/Mobile',
                    'TX': '行動裝置設定'
                }, {
                    'VL': '/Chart',
                    'TX': '圖表程式設定'
                }]
            }),
            queryMode: 'local',
            displayField: 'TX',
            valueField: 'VL',
            editable: false,
            forceSelection: true,
            //fieldCls: 'required',
            //allowBlank: false,
            //allowOnlyWhitespace: false,
            value: '/Form/Index',
            readOnly: true
        }, {
            fieldLabel: 'JS路徑',
            name: 'FU',
            readOnly: true
        }, {
            fieldLabel: 'URL參數',
            name: 'FUP',
            readOnly: true
        }, {
            fieldLabel: '功能說明',
            name: 'FD',
            maxLength: 100,
            enforceMaxLength: true,
            readOnly: true
        }, {
            fieldLabel: '上線',
            name: 'FL',
            xtype: 'checkboxfield',
            allowBlank: false,
            allowOnlyWhitespace: false,
            //fieldCls: 'required',
            readOnly: true,
            inputValue: '1',
            uncheckedValue: '0'
        }, {
            xtype: 'displayfield',
            fieldLabel: '程式最後修改時間',
            name: 'modify',
            hidden: true
        },
        {
            xtype: 'filegrid',
            fieldLabel: '說明文件',
            name: 'ATTACH_URL',
            width: '100%',
            height: 100
        },
        /*
        {
            xtype: 'filebutton',
            fieldLabel: '說明文件(FileBtn)',
            name: 'ATTACH_URL',
            buttonText: '文件數量',
            showPreview: true,
            title: '線上說明文件列表'
        },
        {
            xtype: 'filegrid',
            fieldLabel: '說明文件(FileGrid)',
            name: 'ATTACH_URL',
            width: '100%',
            height: 100
        },
        {
            xtype: 'imagegrid',
            fieldLabel: '說明文件(Grid)',
            name: 'ATTACH_URL2',
            width: '100%',
            height: 100
        },
        */
        {
            name: 'MFG',
            xtype: 'hidden'
        }, {
            name: 'Path',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '文件下載',
            name: 'ATTACH_File',
            hidden: true
        }, {
            xtype: 'numberfield',
            fieldLabel: '檔案上傳上限(KB)',
            name: 'MFS',
            value:4096,
            minValue: 0,
            hidden: true/*,
            allowBlank: false,
            allowOnlyWhitespace: false,
            fieldCls: 'required'*/
        }, ],

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
                }
                );
                } else {
                    Ext.Msg.alert('警告', 'W0021:輸入資料格式有誤');//option
                    msglabel('W0021:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var x1 = f.findField("x").getValue();
            switch (x1) {
                case "A": doAppend(); break;
                case "I": doInsert(); break;
                default:
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    f.submit({
                        url: T1Set,
                        success: function (form, action) {
                            var f2 = this.form;
                            var x = f2.findField("x").getValue();
                            var r = f2.getRecord();

                            switch (x) {
                                case "U":
                                    r.set(action.result.etts[0]);
                                    r.commit();
                                    //selectNode.set('text', action.result.ds.T1[0].text);
                                    //selectNode.commit();
                                    msglabel('G0002:資料修改成功');
                                    break;
                                case "D":
                                    var sq = selectNode.data.FS;
                                    var sn = selectNode.nextSibling;
                                    var n = sn;
                                    while (n !== null) {
                                        n.data.FS = sq;
                                        n.commit();
                                        sq++;
                                        n = n.nextSibling;
                                    }
                                    selectNode.remove();
                                    if (sn !== null) {
                                        selectNode = sn;
                                        T1Tree.getSelectionModel().select(selectNode);
                                    }
                                    msglabel('G0003:資料刪除成功');
                                    break;
                            }
                            myMask.hide();
                            if (x !== 'D') {
                                T1Cleanup(1);
                            }
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
                    break;
            }
        }
    }

    function T1Cleanup() {
        viewport.down('#tree').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.findField('FG').setReadOnly(true);
        f.findField('FT').setReadOnly(true);
        f.findField('F0').setReadOnly(true);
        f.findField('F1').setReadOnly(true);
        f.findField('FA0').setReadOnly(true);
        f.findField('FU').setReadOnly(true);
        f.findField('FUP').setReadOnly(true);
        f.findField('FD').setReadOnly(true);
        f.findField('FL').setReadOnly(true);
        f.findField('ATTACH_URL').setReadOnly(true);
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }
    function setFormT1a() {
        var activeTree = tabs.activeTab.tree;
        activeTree.down('#append').setDisabled(T1Rec === 0);
        activeTree.down('#insert').setDisabled(T1Rec === 0);
        activeTree.down('#edit').setDisabled(T1Rec === 0);
        activeTree.down('#delete').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('FG');
            u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            activeTree.down('#append').setDisabled(f.findField('FT').getValue() === 'L');
            activeTree.down('#insert').setDisabled(f.findField('PG').getValue() === '');
            //T1Tree.down('#delete').setDisabled(f.findField('PG').getValue() === '' || T1LastRec.childNodes.length > 0);
            activeTree.down('#delete').setDisabled(f.findField('PG').getValue() === '');
            if (T1LastRec.childNodes !== null) {
                if (T1LastRec.childNodes.length > 0) {
                    activeTree.down('#delete').setDisabled(true);
                }
            }
        } else {
            T1Form.getForm().reset();
        }
    }
    
    var tabs = Ext.widget('tabpanel', {
        plain: true,
        resizeTabs: true,
        deferredRender: false,
        cls: 'tabpanel',
        defaults: {
            layout: 'fit',
        },
        items: [{
            id: 'tabClassic',
            title: '所有程式',
            items: [T1Tree],
            tree: T1Tree,
            closable: false
        }, {
            id: 'tabMobile',
            title: '行動版程式',
            //items: [T2Tree],
            //tree: T2Tree,
            closable: false,
            hidden: true
        }],
        listeners: {
            //頁籤字體改變顏色 by 吉威 2018/10/19
            tabchange: function (tabPanel, newCard, oldCard, eOpts) {
                oldCard.tree.getSelectionModel().deselectAll();
                T1Rec = 0;
                T1LastRec = null;
                setFormT1a();
                if (oldCard) {
                    oldCard.tab.btnInnerEl.setStyle('color', 'black');
                }
                if (newCard) {
                    newCard.tab.btnInnerEl.setStyle('color', 'brown');
                }
            }
        }
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
            items: [tabs]
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

    var myMask = new Ext.LoadMask(viewport, { msg: '請稍後...' });

    function hideMask() {
        myMask.hide();
    }

    function T1Load() {
        try {
            myMask.show();
            T1Store.load({
                params: {
                    PG: null
                },
                callback: function () {
                    myMask.hide();
                }
            });
            /*
            T1Store.on('refresh', function () {
                T1Tree.fireEvent('viewready');
            });*/
        }
        catch (e) {
            alert('T1Load Error:' + e.message);
        }
    }
    IsPageLoad = false;
    T1Load();
});
