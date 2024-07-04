
Ext.onReady(function () {
    var T1Get = '../../../api/NAFile/GetByKey';
    var Ext1Get = '../../../api/NAFile/GetEC';
    var T1Set = '../../../api/flow/upload/TraMMUploadSet';
    var T1Name = "";
    var T1Rec = 0;
    var T1LastRec = null;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    var fp = '/Upload/Files';
    var ua = (Ext.getUrlParam('UA') == '1') ? true : false;
    var da = (Ext.getUrlParam('DA') == '1') ? true : false;
    var uk = Ext.getUrlParam('UK');
    var ec = Ext.getUrlParam('EC');
    var T1Name = Ext.getUrlParam('wTitle');//20130617 Dan add

    var ec_msg = "";
    var ec_msg_red = "";
    var ec_arr = new Array();

    var isFocus = false;
    var isStartEdit = false;
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('Ext1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'NAME', type: 'string' },
            { name: 'VALUE', type: 'string' },
        ]
    });

    var Ext1Store = Ext.create('Ext.data.Store', {
        model: 'Ext1Model',
        pageSize: 10,
        autoLoad: true,
        sorters: [{ property: 'VALUE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    EC: ec
                };
                Ext.apply(store.proxy.extraParams, np);
            }, load: function (store) {
                var d = store.data;
                for (i = 0; i < d.length; i++) {
                    var v = d.get(i).get('VALUE')
                    if (i > 0) { ec_msg += "、"; ec_msg_red += "、"; }
                    ec_msg += v;
                    ec_msg_red += "<span style='color:red'>" + v + '</span>';
                    ec_arr.push(v);
                }
                T1Grid.setTitle('請注意，上傳格式限制為：' + ec_msg_red + '。');
            }

        },
        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: Ext1Get,
            reader: {
                type: 'json',
                root: 'etts'
            }
        }
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'UK', type: 'string' },
            { name: 'FN', type: 'string' },//附加檔案 PIC_ATTACH_NAME
            { name: 'FD', type: 'string' },//檔案說明
            { name: 'FT', type: 'string' },//副檔
            { name: 'FP', type: 'string' },//附加檔案位址 PIC_ATTACH_URL
            { name: 'FG', type: 'string' },//file guid FG
            { name: 'FC', type: 'date' },//建立日期
            { name: 'UNA', type: 'string' }//建立人員
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10,
        autoLoad: true,
        sorters: [{ property: 'FC', direction: 'DESC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    UK: uk
                };
                Ext.apply(store.proxy.extraParams, np);
            }, load: function (store) {
                if (isFocus) {
                    T1Grid.getView().select(0);
                    isFocus = false;
                }
                
                if (isStartEdit) {
                    T1Grid.plugins[0].startEditByPosition({
                        row: 0,
                        column: 2
                    });
                    isStartEdit = false;
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: 'POST',
            url: T1Get,
            reader: {
                type: 'json',
                root: 'etts'
            }
        }
    });

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '1 1 1',
        fieldDefaults: {
            msgTarget: 'side',
            labelAlign: "right",
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'UK',
            xtype: 'hidden'
        }, {
            xtype: 'container',
            layout: 'column',
            width: 700,
            defaults: {
                layout: 'anchor',
                defaults: {
                    anchor: '100%',
                    labelAlign: "right"
                }
            },
            items: [{
                fieldLabel: 'GUID',
                name: 'FG',
                xtype: 'hidden'
            }, {
                name: 'DelFG',
                xtype: 'hidden'
            }, {
                xtype: 'container',
                columnWidth: .52,
                items: []
            }
            ]
        }],

        buttons: [
            {
                itemId: 'T1Browse2', text: '瀏覽...', handler: function () {
                    $('#file')[0].click();
                }
            },
            {
                itemId: 'T1Submit', text: '上傳', hidden: true, handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        Ext.MessageBox.confirm('上傳', '是否確定上傳?', function (btn, text) {
                            if (btn === 'yes') {
                                if (FileExists()) {
                                    Ext.MessageBox.confirm('提醒', '檔案已存在，是否覆蓋?', function (btn, text) {
                                        if (btn === 'yes') {
                                            T1Submit();
                                        }
                                    }
                                    );
                                } else {
                                    T1Submit();
                                }
                            }
                        }
                        );
                    }
                }
            }, {
                itemId: 'T1Cancel', text: '取消', hidden: true, handler: T1Cleanup
            }]
        , listeners: {
            afterrender: function (element) {
                var fields = T1Form.getForm().getFields();
                Ext.each(fields.items, function (f) {
                    f.setReadOnly(true);
                });
            }
        }
    });

    function FileExists() {
        var f = T1Form.getForm().findField('fileupload1').getValue();
        var pos = f.lastIndexOf("\\");
        var fname = Ext.util.Format.uppercase(f.substring(pos + 1));

        T1Form.getForm().findField('DelFG').setValue("");
        for (var i = 0; i < T1Store.data.length; i++) {
            if (fname == Ext.util.Format.uppercase(T1Store.data.items[i].data.FN)) {
                T1Form.getForm().findField('DelFG').setValue(T1Store.data.items[i].data.FG);
                break;
            }
        };
        if (T1Form.getForm().findField('DelFG').getValue() == "") return false;
        else return true;
    }

    function UploadFiles() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        //## 宣告一個FormData
        var data = new FormData();
        //## 將檔案append FormData
        var files = $('#file').get(0).files;
        var fLen = files.length;
        if (fLen > 0) {
            for (var i = 0; i < fLen; i++) {
                data.append('[' + i + "].file", files[i]);
            }
        }

        //## 透過ajax方式Post 至Action
        $.ajax({
            type: "POST",
            url: '../../../api/NAFile/Upload',
            contentType: false,
            processData: false,
            dataType: "json",
            data: data,
            beforeSend: function (request) {
                request.setRequestHeader('EC', ec)
                request.setRequestHeader('FP', fp)
                request.setRequestHeader('UK', uk)
            },
        })
            .done(function (data, textStatus) {
                //alert(data);
                parent.$('#DivResult')[0].innerText = data.msg;
                myMask.hide();
                isFocus = true;
                isStartEdit = true;
                T1Store.load();
                var btnUpload = T1Tool.queryById('T1Upload');
                btnUpload.setText('上傳');
                btnUpload.setDisabled(true);
                document.getElementById("file").value = '';
            })
            .fail(function (data, textStatus) {
                //alert("錯誤:" + data);
                parent.$('#DivResult')[0].innerText = "錯誤:" + data.msg;
                myMask.hide();
            });
    }

    function DeleteFile() {
        var fg = T1Grid.getSelectionModel().selected.items[0].data.FG;
        //alert('del ' + fg);
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        //## 透過ajax方式Post 至Action
        $.ajax({
            type: "POST",
            url: '../../../api/NAFile/Delete',
            dataType: "json",
            data: { FG: fg }
        })
            .done(function (data, textStatus) {
                parent.$('#DivResult')[0].innerText = data.msg;
                myMask.hide();
                T1Store.load({
                    callback: function () {
                        T1Grid.down('#delete').setDisabled(T1Grid.getSelectionModel().selected.items.length === 0);
                    }
                });
            })
            .fail(function (data, textStatus) {
                parent.$('#DivResult')[0].innerText = "錯誤:" + data.msg;
                myMask.hide();
                T1Grid.down('#delete').setDisabled(T1Grid.getSelectionModel().selected.items.length === 0);
            });
    }

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
                        if (T1Form.getForm().findField('DelFG').getValue() == "") {
                            r.set(action.result.ds.T1[0]);
                            T1Store.insert(0, r);
                            r.commit();
                            T1Tool.moveFirst();
                        } else {
                            T1Load();
                        }
                        isFocus = true;
                        //msglabel('G0001:資料新增成功');
                        break;
                    case "D":
                        //T1Load();
                        //msglabel('G0003:資料刪除成功');
                        T1Store.remove(r);
                        r.commit();
                        T1Tool.doRefresh();
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

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                cls: 'funBtn',
                margin: '0 4 0 0',
                itemId: 'T1Browse', text: '瀏覽', hidden: !ua, handler: function () {
                    $('#file')[0].click();
                }
            },
            {
                cls: 'funBtn',
                itemId: 'T1Upload', text: '上傳', disabled: true, hidden: !ua,
                handler: function () {
                    UploadFiles();
                }
            },
            {
                itemId: 'insert',
                text: '新增',
                hidden: true,
                handler: function () {
                    setFormT1('I', '新增');
                }
            },
            {
                cls: 'funBtn',
                itemId: 'delete',
                text: '刪除',
                disabled: true,
                hidden: !da,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            //T1Form.getForm().findField('x').setValue('D');
                            //T1Submit();
                            DeleteFile();
                        }
                    }
                    );
                }
            }
        ]
    });
    function T1Cleanup() {
        viewport.down('#T1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        viewport.down('#form').setVisible(false);
        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            f.setReadOnly(true);
        });

        T1Form.down('#T1Cancel').hide();
        T1Form.down('#T1Submit').hide();
        //T1Form.down('#fUpload').setDisabled(true);
        //viewport.down('#form').setTitle('瀏覽');

        T1Rec = 0;
        T1LastRec = null;
        setFormT1a();
    }

    function setFormT1(i, t) {
        viewport.down('#T1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();

        var fields = T1Form.getForm().getFields();
        Ext.each(fields.items, function (f) {
            f.setReadOnly(false);
        });

        var f = T1Form.getForm();
        if (i === "I") {
            var r = Ext.create('T1Model');
            f.loadRecord(r);
            f.findField('x').setValue('I');
            u = f.findField('fileupload1');
            //T1Form.down('#fUpload').setDisabled(false);
            //f.findField('fUpload').allowBlank = false;
            //改成document.getElementById('file')[0]
            viewport.down('#form').setVisible(true);
            f.findField('UK').setValue(uk);
            viewport.down('#form').setTitle('瀏覽');
        }
        /*2013/4/18 else if (i === "U") {
            f.findField('x').setValue('U');
            u = f.findField('fileupload1');
            //if update,key field set readonly
            //f.findField('PR_NO').setReadOnly(true);   
        }*/

        T1Form.down('#T1Cancel').setVisible(true);
        T1Form.down('#T1Submit').setVisible(true);
        u.focus();
    }

    var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1,
        autoCancel: false,
        listeners: {
            edit: function (editor, context, eOpts) {
                Ext.Ajax.request({
                    url: '../../../api/NAFile/UpdateFD',
                    method: reqVal_p,
                    params: {
                        FG: context.record.data['FG'],
                        FD: context.record.data['FD']
                    },
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            parent.$('#DivResult')[0].innerText = "檔案說明已儲存";
                        }
                    },
                    failure: function (response) {
                        parent.$('#DivResult')[0].innerText = "檔案說明發生例外錯誤";
                    }
                });
            },
            canceledit: function (editor, context, eOpts) {
            }
        }
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [cellEditing],

        dockedItems: [
            {
                html: '<input type="file" name="file" id="file" multiple id="file" style="display:none;" onchange="javascript:file_change(this);" /> '
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],

        columns: [{ xtype: 'rownumberer' },
        {
            text: "附加檔案",
            dataIndex: 'FN',
            flex: 1,
            renderer: function (val, meta, record) {
                return '<a href=javascript:DownloadFileNA("' + record.get('FG') + '");>' + val + '<img src="../../../Images/TRA/save.gif" align="absmiddle" /></a>';
                //return '<a href=javascript:getFileName("' + record.get('FG') + '");>' + val + '</a>';
            }
            }, {
                text: "檔案說明",
                dataIndex: 'FD',
                flex: 2,
                width: 90,
                editor: {
                    emptyText: '請在此輸入檔案說明...'
                }
            }, {
            xtype: 'datecolumn',
            text: "上傳日期",
            dataIndex: 'FC',
            format: 'Y/m/d H:i:s',
            width: 200,
            flex: 1
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
        //T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);

        if (T1LastRec) {
            T1Form.loadRecord(T1LastRec);
            //var f = T1Form.getForm();
            //f.findField('x').setValue('U');
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
            itemId: 'T1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            split: true,
            width: '67%',
            minWidth: 50,
            minHeight: 140,
            border: false,
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'south',
            collapsible: true,
            floatable: true,
            height: '35%',
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
    viewport.down('#form').setVisible(false);
    if (Ext.getUrlParam('MODE') == 'R' | uk == "" | uk == undefined) {
        T1Grid.down('#insert').setVisible(false);
        T1Grid.down('#delete').setVisible(false);
    }

    var fileElement = document.getElementById("file");
    fileElement.onchange = function (event) {
        var target = event.target || event.srcElement;
        var btnUpload = T1Tool.queryById('T1Upload');
        if (target.value.length == 0) {
            btnUpload.setText('上傳');
            btnUpload.setDisabled(true);
        } else {
            var file_ext_valid = true;
            
            var len = target.files.length;
            for (i = 0; i < len; i++)
            {
                var fn = target.files[i].name;
                var di = fn.lastIndexOf(".");
                if (di < 0)
                {
                    file_ext_valid = false;
                }
                else
                {
                    var ext = fn.substring(di + 1).toUpperCase();
                    file_ext_valid = (ec_arr.indexOf(ext) > -1);
                }

                if (!file_ext_valid)
                {
                    var w = Ext.Msg.show({
                        title: '上傳檔案副檔名有誤',
                        msg: '無法上傳 ' + fn + ', 須為:' + ec_msg_red + '。',
                        buttons: Ext.Msg.OK,
                        icon: Ext.MessageBox.WARNING,
                        cls: 'warnMsg'
                    });
                    parent.$('#DivResult')[0].innerText = '無法上傳 ' + fn + ', 須為:' + ec_msg + '。';
                    break;
                }
            }
            
            if (file_ext_valid) {
                parent.$('#DivResult')[0].innerText = '';
                btnUpload.setText('上傳(<span style="color:red;">' + target.files.length + '</span>)');
                btnUpload.setDisabled(false);
            }
            else
            {
                btnUpload.setText('上傳');
                btnUpload.setDisabled(true);
            }
        }
    }
});